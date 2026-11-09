namespace Clubes.Shared;

public class Clube
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Sigla { get; set; } = "";
    public string Endereco { get; set; } = "";
    public string Bairro { get; set; } = "";
    public string Cidade { get; set; } = "";
    public string UF { get; set; } = "";
    public string Fone { get; set; } = "";
    public string Email { get; set; } = "";
    public decimal ValorMensalidade { get; set; }
}

public class Associado
{
    public int Id { get; set; }
    public int ClubeId { get; set; }
    public string Nome { get; set; } = "";
    public string CPF { get; set; } = "";
    public string Endereco { get; set; } = "";
    public string Bairro { get; set; } = "";
    public Sexo Sexo { get; set; }
    public EstadoCivil EstadoCivil { get; set; }
    public string Cidade { get; set; } = "";
    public string UF { get; set; } = "";
    public string Fone { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime DataNascimento { get; set; }
    public List<string> Interesses { get; set; } = new();
    public TipoAssociado Tipo { get; set; }
    public StatusAprovacao Aprovacao { get; set; } = StatusAprovacao.Pendente;
    public List<Dependente> Dependentes { get; set; } = new();

    // Só fundador e associado votam.
    public bool TemDireitoVoto => Tipo is TipoAssociado.SocioFundador or TipoAssociado.SocioAssociado;
    public bool PrecisaAprovacao => Tipo is TipoAssociado.SocioConvidado or TipoAssociado.Simpatizante;
}

public class Dependente
{
    public int Id { get; set; }
    public int AssociadoId { get; set; }
    public string Nome { get; set; } = "";
    public string Parentesco { get; set; } = "";
    public DateTime DataNascimento { get; set; }
    public bool TemAplicativo { get; set; }
    public string Email { get; set; } = "";   // login do dependente, quando tem acesso
}

public class Evento
{
    public int Id { get; set; }
    public int ClubeId { get; set; }
    public string Titulo { get; set; } = "";
    public string Descricao { get; set; } = "";
    public DateTime DataHora { get; set; }
    public PublicoEvento Publico { get; set; }
}

public class Mensalidade
{
    public int Id { get; set; }
    public int AssociadoId { get; set; }
    public DateTime Competencia { get; set; }
    public DateTime Vencimento { get; set; }
    public decimal Valor { get; set; }
    public DateTime? DataPagamento { get; set; }
    public SituacaoMensalidade Situacao =>
        DataPagamento.HasValue ? SituacaoMensalidade.Paga :
        Vencimento.Date < DateTime.Today ? SituacaoMensalidade.Atrasada : SituacaoMensalidade.Pendente;
}

public class ReservaSalao
{
    public int Id { get; set; }
    public int ClubeId { get; set; }
    public int AssociadoId { get; set; }   // 0 = reserva interna do clube
    public FinalidadeReserva Finalidade { get; set; }
    public string Descricao { get; set; } = "";
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public StatusAprovacao Status { get; set; } = StatusAprovacao.Pendente;
    public string Periodo => $"{Inicio:dd/MM HH:mm} a {Fim:dd/MM HH:mm}";
}

public class Solicitacao
{
    public int Id { get; set; }
    public int ClubeId { get; set; }
    public int AssociadoId { get; set; }
    public TipoSolicitacao Tipo { get; set; }
    public string Assunto { get; set; } = "";
    public string Texto { get; set; } = "";
    public DateTime CriadaEm { get; set; } = DateTime.Now;
    public string Resposta { get; set; } = "";
    public bool Respondida { get; set; }
}
