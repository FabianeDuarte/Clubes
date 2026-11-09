using System.Security.Claims;
using Clubes.Server.Data;
using Clubes.Shared;
using Microsoft.EntityFrameworkCore;
namespace Clubes.Server.Api;

public static class ApiEndpoints
{
    static IResult Falha(string msg, int status = 400) => Results.Json(new Erro(msg), statusCode: status);
    static int? AssocId(ClaimsPrincipal u) => int.TryParse(u.FindFirstValue("associadoId"), out var id) ? id : null;

    // Associado logado E aprovado pelo clube (null se não).
    static async Task<Associado?> Aprovado(ClaimsPrincipal u, ClubesDb db)
    {
        if (EhDependente(u) || AssocId(u) is not int id) return null;
        var a = await db.Associados.FindAsync(id);
        return a is { Aprovacao: StatusAprovacao.Aprovado } ? a : null;
    }
    const string NaoAprovado = "Seu cadastro ainda não foi aprovado pelo clube.";
    static bool EhDependente(ClaimsPrincipal u) => u.FindFirstValue("dependenteId") is not null;
    static IResult Negado(ClaimsPrincipal u) =>
        Falha(EhDependente(u) ? "Conta de dependente: acesso somente à consulta de eventos." : NaoAprovado, 403);

    public static void MapApi(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        // ---------- público ----------
        api.MapGet("/clubes", async (ClubesDb db) =>
            Results.Ok(await db.Clubes.AsNoTracking().OrderBy(c => c.Nome).ToListAsync()));

        api.MapGet("/clubes/{id:int}/eventos", async (int id, ClubesDb db) =>
            Results.Ok(await db.Eventos.AsNoTracking()
                .Where(e => e.ClubeId == id && e.Publico == PublicoEvento.AbertoAoPublico && e.DataHora >= DateTime.Today)
                .OrderBy(e => e.DataHora).ToListAsync()));

        api.MapGet("/clubes/{id:int}/agenda", async (int id, ClubesDb db) =>
        {
            var l = await db.Reservas.AsNoTracking()
                .Where(r => r.ClubeId == id && r.Status == StatusAprovacao.Aprovado && r.Fim >= DateTime.Today)
                .OrderBy(r => r.Inicio).ToListAsync();
            return Results.Ok(l.Select(r => new AgendaItem(r.Descricao, r.Finalidade, r.Inicio, r.Fim)));
        });

        api.MapPost("/auth/login", async (LoginRequest r, ClubesDb db, TokenService tk) =>
        {
            var email = (r.Email ?? "").Trim().ToLowerInvariant();
            var u = await db.Usuarios.FirstOrDefaultAsync(x => x.Email == email);
            if (u is null || !Senhas.Confere(u, r.Senha ?? "")) return Falha("E-mail ou senha inválidos.", 401);
            return Results.Ok(new LoginResponse(tk.Gerar(u), u.Nome, u.Perfil, u.ClubeId, u.AssociadoId));
        });

        api.MapPost("/auth/cadastro", async (CadastroRequest r, ClubesDb db) =>
        {
            if (!await db.Clubes.AnyAsync(c => c.Id == r.ClubeId)) return Falha("Clube inválido.");
            if (string.IsNullOrWhiteSpace(r.Nome)) return Falha("Informe o nome.");
            if (!Cpf.Valido(r.Cpf)) return Falha("CPF inválido.");
            var email = (r.Email ?? "").Trim().ToLowerInvariant();
            if (!email.Contains('@')) return Falha("E-mail inválido.");
            if ((r.Senha ?? "").Length < 6) return Falha("A senha precisa ter ao menos 6 caracteres.");
            if (await db.Usuarios.AnyAsync(x => x.Email == email)) return Falha("Este e-mail já está cadastrado.");
            var cpf = Cpf.Limpar(r.Cpf);
            if (await db.Associados.AnyAsync(x => x.ClubeId == r.ClubeId && x.CPF == cpf)) return Falha("CPF já cadastrado neste clube.");

            // Todo autocadastro entra PENDENTE (inclusive fundador/associado, que votam): o clube aprova no painel.
            var novo = new Associado
            {
                ClubeId = r.ClubeId, Nome = r.Nome.Trim(), CPF = cpf, Email = email, Fone = r.Fone ?? "",
                Endereco = r.Endereco ?? "", Bairro = r.Bairro ?? "", Cidade = r.Cidade ?? "", UF = (r.UF ?? "").ToUpperInvariant(),
                Sexo = r.Sexo, EstadoCivil = r.EstadoCivil, DataNascimento = r.DataNascimento,
                Interesses = r.Interesses ?? new(), Tipo = r.Tipo, Aprovacao = StatusAprovacao.Pendente
            };
            db.Associados.Add(novo);
            await db.SaveChangesAsync();
            var u = new Usuario { Nome = novo.Nome, Email = email, Perfil = PerfilUsuario.Associado, ClubeId = r.ClubeId, AssociadoId = novo.Id };
            u.SenhaHash = Senhas.Hash(u, r.Senha!);
            db.Usuarios.Add(u);
            await db.SaveChangesAsync();
            return Results.Ok(new { novo.Id, Aprovacao = novo.Aprovacao.ToString() });
        });

        // ---------- autenticado (JWT) ----------
        var m = api.MapGroup("/meus").RequireAuthorization("Api");

        m.MapGet("/perfil", async (ClaimsPrincipal u, ClubesDb db) =>
        {
            if (AssocId(u) is not int id || EhDependente(u)) return Falha("Esta conta não é de associado.", 403);
            var a = await db.Associados.AsNoTracking().Include(x => x.Dependentes).FirstOrDefaultAsync(x => x.Id == id);
            return a is null ? Falha("Associado não encontrado.", 404) : Results.Ok(a);
        });

        m.MapGet("/mensalidades", async (ClaimsPrincipal u, ClubesDb db) =>
        {
            if (await Aprovado(u, db) is not { } a) return Negado(u);
            return Results.Ok(await db.Mensalidades.AsNoTracking().Where(x => x.AssociadoId == a.Id).OrderBy(x => x.Competencia).ToListAsync());
        });

        // Sócio aprovado (exceto simpatizante) e seus dependentes veem todos os eventos do clube;
        // pendentes e simpatizantes veem só os abertos ao público.
        m.MapGet("/eventos", async (ClaimsPrincipal u, ClubesDb db) =>
        {
            if (AssocId(u) is not int id || !int.TryParse(u.FindFirstValue("clubeId"), out var clubeId)) return Falha("Conta inválida.", 403);
            var titular = await db.Associados.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            bool membro = titular is { Aprovacao: StatusAprovacao.Aprovado } && titular.Tipo != TipoAssociado.Simpatizante;
            var q = db.Eventos.AsNoTracking().Where(e => e.ClubeId == clubeId && e.DataHora >= DateTime.Today);
            if (!membro) q = q.Where(e => e.Publico == PublicoEvento.AbertoAoPublico);
            return Results.Ok(await q.OrderBy(e => e.DataHora).ToListAsync());
        });

        m.MapGet("/reservas", async (ClaimsPrincipal u, ClubesDb db) =>
        {
            if (await Aprovado(u, db) is not { } a) return Negado(u);
            return Results.Ok(await db.Reservas.AsNoTracking().Where(r => r.AssociadoId == a.Id).OrderByDescending(r => r.Inicio).ToListAsync());
        });

        m.MapPost("/reservas", async (ReservaRequest r, ClaimsPrincipal u, ClubesDb db) =>
        {
            if (await Aprovado(u, db) is not { } a) return Negado(u);
            if (r.Fim <= r.Inicio) return Falha("O fim deve ser depois do início.");
            if (r.Inicio < DateTime.Now) return Falha("O início deve ser no futuro.");
            if (await Regras.TemConflito(db, a.ClubeId, r.Inicio, r.Fim)) return Falha("Já existe reserva neste período.");
            db.Reservas.Add(new ReservaSalao { ClubeId = a.ClubeId, AssociadoId = a.Id, Finalidade = r.Finalidade,
                Descricao = r.Descricao ?? "", Inicio = r.Inicio, Fim = r.Fim, Status = StatusAprovacao.Pendente });
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        m.MapGet("/solicitacoes", async (ClaimsPrincipal u, ClubesDb db) =>
        {
            if (await Aprovado(u, db) is not { } a) return Negado(u);
            return Results.Ok(await db.Solicitacoes.AsNoTracking().Where(s => s.AssociadoId == a.Id).OrderByDescending(s => s.CriadaEm).ToListAsync());
        });

        m.MapPost("/solicitacoes", async (SolicitacaoRequest r, ClaimsPrincipal u, ClubesDb db) =>
        {
            if (await Aprovado(u, db) is not { } a) return Negado(u);
            if (string.IsNullOrWhiteSpace(r.Assunto) || string.IsNullOrWhiteSpace(r.Texto)) return Falha("Informe assunto e descrição.");
            db.Solicitacoes.Add(new Solicitacao { ClubeId = a.ClubeId, AssociadoId = a.Id, Tipo = r.Tipo, Assunto = r.Assunto.Trim(), Texto = r.Texto.Trim() });
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        m.MapPost("/dependentes", async (Dependente d, ClaimsPrincipal u, ClubesDb db) =>
        {
            if (await Aprovado(u, db) is not { } a) return Negado(u);
            if (string.IsNullOrWhiteSpace(d.Nome)) return Falha("Informe o nome do dependente.");
            db.Dependentes.Add(new Dependente { AssociadoId = a.Id, Nome = d.Nome.Trim(), Parentesco = d.Parentesco ?? "",
                DataNascimento = d.DataNascimento, TemAplicativo = d.TemAplicativo });
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        // Titular cria o login do dependente (somente consulta de eventos).
        m.MapPost("/dependentes/{id:int}/acesso", async (int id, AcessoDependenteRequest r, ClaimsPrincipal u, ClubesDb db) =>
        {
            if (await Aprovado(u, db) is not { } a) return Negado(u);
            var d = await db.Dependentes.FirstOrDefaultAsync(x => x.Id == id && x.AssociadoId == a.Id);
            if (d is null) return Falha("Dependente não encontrado.", 404);
            var email = (r.Email ?? "").Trim().ToLowerInvariant();
            if (!email.Contains('@')) return Falha("E-mail inválido.");
            if ((r.Senha ?? "").Length < 6) return Falha("A senha precisa ter ao menos 6 caracteres.");
            if (await db.Usuarios.AnyAsync(x => x.DependenteId == d.Id)) return Falha("Este dependente já tem acesso.");
            if (await db.Usuarios.AnyAsync(x => x.Email == email)) return Falha("Este e-mail já está cadastrado.");
            var novo = new Usuario { Nome = d.Nome, Email = email, Perfil = PerfilUsuario.Dependente, ClubeId = a.ClubeId, AssociadoId = a.Id, DependenteId = d.Id };
            novo.SenhaHash = Senhas.Hash(novo, r.Senha!);
            db.Usuarios.Add(novo);
            d.TemAplicativo = true; d.Email = email;
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        m.MapDelete("/dependentes/{id:int}", async (int id, ClaimsPrincipal u, ClubesDb db) =>
        {
            if (await Aprovado(u, db) is not { } a) return Negado(u);
            var d = await db.Dependentes.FirstOrDefaultAsync(x => x.Id == id && x.AssociadoId == a.Id);
            if (d is null) return Falha("Dependente não encontrado.", 404);
            db.Usuarios.RemoveRange(await db.Usuarios.Where(x => x.DependenteId == d.Id).ToListAsync());
            db.Dependentes.Remove(d);
            await db.SaveChangesAsync();
            return Results.Ok();
        });
    }
}
