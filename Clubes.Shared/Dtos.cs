namespace Clubes.Shared;

public record Erro(string Mensagem);
public record LoginRequest(string Email, string Senha);
public record LoginResponse(string Token, string Nome, PerfilUsuario Perfil, int? ClubeId, int? AssociadoId);

public record CadastroRequest(
    int ClubeId, string Nome, string Cpf, string Email, string Senha, string Fone,
    string Endereco, string Bairro, string Cidade, string UF,
    Sexo Sexo, EstadoCivil EstadoCivil, DateTime DataNascimento,
    List<string> Interesses, TipoAssociado Tipo);

public record ReservaRequest(FinalidadeReserva Finalidade, string Descricao, DateTime Inicio, DateTime Fim);
public record AcessoDependenteRequest(string Email, string Senha);
public record SolicitacaoRequest(TipoSolicitacao Tipo, string Assunto, string Texto);

// Agenda pública de locações (sem dados pessoais).
public record AgendaItem(string Descricao, FinalidadeReserva Finalidade, DateTime Inicio, DateTime Fim)
{
    public string Periodo => $"{Inicio:dd/MM HH:mm} a {Fim:dd/MM HH:mm}";
}
