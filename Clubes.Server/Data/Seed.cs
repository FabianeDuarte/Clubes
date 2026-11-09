using Clubes.Shared;
namespace Clubes.Server.Data;

public static class Seed
{
    public static void Run(ClubesDb db)
    {
        if (db.Clubes.Any()) return;

        var foz = new Clube { Nome = "Clube Foz 2", Sigla = "FOZ2", Cidade = "Foz do Iguaçu", UF = "PR", ValorMensalidade = 80 };
        var gua = new Clube { Nome = "Clube Guaíra 2", Sigla = "GUA2", Cidade = "Guaíra", UF = "PR", ValorMensalidade = 70 };
        db.Clubes.AddRange(foz, gua);
        db.SaveChanges();

        var joao = new Associado
        {
            ClubeId = foz.Id, Nome = "João da Silva", CPF = "52998224725", Email = "joao@clubes.local",
            Cidade = "Foz do Iguaçu", UF = "PR", DataNascimento = new DateTime(1980, 5, 10),
            Interesses = new() { "pesca", "churrasco" }, Tipo = TipoAssociado.SocioFundador,
            Aprovacao = StatusAprovacao.Aprovado
        };
        db.Associados.Add(joao);
        db.SaveChanges();

        // Senhas de DEMONSTRAÇÃO: troque antes de usar de verdade.
        db.Usuarios.AddRange(
            Novo("Super Admin", "admin@clubes.local", "Admin@123", PerfilUsuario.SuperAdmin, null, null),
            Novo("Presidente Foz 2", "presidente.foz2@clubes.local", "Clube@123", PerfilUsuario.PresidenteAdmin, foz.Id, null),
            Novo("Tesoureiro Foz 2", "tesoureiro.foz2@clubes.local", "Clube@123", PerfilUsuario.Tesoureiro, foz.Id, null),
            Novo("João da Silva", "joao@clubes.local", "Socio@123", PerfilUsuario.Associado, foz.Id, joao.Id));

        var hoje = DateTime.Today;
        for (int i = -1; i <= 1; i++)
        {
            var comp = new DateTime(hoje.Year, hoje.Month, 1).AddMonths(i);
            db.Mensalidades.Add(new Mensalidade { AssociadoId = joao.Id, Competencia = comp, Vencimento = comp.AddDays(9),
                Valor = foz.ValorMensalidade, DataPagamento = i < 0 ? comp.AddDays(5) : null });
        }
        db.Eventos.AddRange(
            new Evento { ClubeId = foz.Id, Titulo = "Café da tarde", DataHora = hoje.AddDays(10).AddHours(16), Publico = PublicoEvento.AbertoAoPublico },
            new Evento { ClubeId = foz.Id, Titulo = "Jantar dia dos Pais", DataHora = hoje.AddDays(25).AddHours(20), Publico = PublicoEvento.SomenteAssociados });
        db.Reservas.Add(new ReservaSalao { ClubeId = foz.Id, Finalidade = FinalidadeReserva.Feijoada, Descricao = "Feijoada beneficente",
            Inicio = hoje.AddDays(30).AddHours(11), Fim = hoje.AddDays(31).AddHours(16), Status = StatusAprovacao.Aprovado });
        db.SaveChanges();
    }

    static Usuario Novo(string nome, string email, string senha, PerfilUsuario perfil, int? clube, int? assoc)
    {
        var u = new Usuario { Nome = nome, Email = email, Perfil = perfil, ClubeId = clube, AssociadoId = assoc };
        u.SenhaHash = Senhas.Hash(u, senha);
        return u;
    }
}
