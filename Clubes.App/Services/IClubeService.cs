using Clubes.Shared;
namespace Clubes.App.Services;

public interface IClubeService
{
    Task<List<Clube>> GetClubesAsync();
    Task<(bool ok, string msg)> LoginAsync(string email, string senha);
    void Sair();
    Task<(bool ok, string msg)> CadastrarAsync(CadastroRequest r);

    Task<List<Evento>> GetEventosAsync(int clubeId);          // logado: todos do clube; anônimo: só abertos
    Task<List<AgendaItem>> GetAgendaAsync(int clubeId);
    Task<List<Mensalidade>> GetMensalidadesAsync();
    Task<(bool ok, string msg)> SolicitarReservaAsync(ReservaRequest r);
    Task<List<ReservaSalao>> GetMinhasReservasAsync();
    Task<(bool ok, string msg)> EnviarSolicitacaoAsync(SolicitacaoRequest r);
    Task<List<Solicitacao>> GetMinhasSolicitacoesAsync();
    Task<Associado?> GetPerfilAsync();
    Task<(bool ok, string msg)> AddDependenteAsync(Dependente d);
    Task<(bool ok, string msg)> RemoverDependenteAsync(int id);
    Task<(bool ok, string msg)> CriarAcessoDependenteAsync(int dependenteId, string email, string senha);
}
