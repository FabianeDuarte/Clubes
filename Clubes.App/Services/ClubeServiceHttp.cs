using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Clubes.App.ViewModels;
using Clubes.Shared;
namespace Clubes.App.Services;

public static class ApiConfig
{
    // Emulador Android enxerga o PC em 10.0.2.2. Em aparelho real, use o IP do PC na rede (ex.: http://192.168.0.10:5080).
    public static string BaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5080" : "http://localhost:5080";
}

public class ClubeServiceHttp(HttpClient http, AppState state) : IClubeService
{
    const string SemConexao = "Não foi possível conectar ao servidor.";

    public async Task<List<Clube>> GetClubesAsync() => await ListAsync<Clube>("api/clubes");

    public async Task<(bool ok, string msg)> LoginAsync(string email, string senha)
    {
        try
        {
            var r = await http.PostAsJsonAsync("api/auth/login", new LoginRequest(email, senha));
            if (!r.IsSuccessStatusCode) return (false, await LerErro(r));
            var s = await r.Content.ReadFromJsonAsync<LoginResponse>();
            if (s is null) return (false, "Resposta inválida do servidor.");
            state.Sessao = s;
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", s.Token);
            return (true, "");
        }
        catch (HttpRequestException) { return (false, SemConexao); }
    }

    public void Sair()
    {
        state.Sessao = null;
        http.DefaultRequestHeaders.Authorization = null;
    }

    public Task<(bool ok, string msg)> CadastrarAsync(CadastroRequest r) => PostAsync("api/auth/cadastro", r);

    public Task<List<Evento>> GetEventosAsync(int clubeId) =>
        state.Logado ? ListAsync<Evento>("api/meus/eventos") : ListAsync<Evento>($"api/clubes/{clubeId}/eventos");

    public Task<List<AgendaItem>> GetAgendaAsync(int clubeId) => ListAsync<AgendaItem>($"api/clubes/{clubeId}/agenda");
    public Task<List<Mensalidade>> GetMensalidadesAsync() => ListAsync<Mensalidade>("api/meus/mensalidades");
    public Task<(bool ok, string msg)> SolicitarReservaAsync(ReservaRequest r) => PostAsync("api/meus/reservas", r);
    public Task<List<ReservaSalao>> GetMinhasReservasAsync() => ListAsync<ReservaSalao>("api/meus/reservas");
    public Task<(bool ok, string msg)> EnviarSolicitacaoAsync(SolicitacaoRequest r) => PostAsync("api/meus/solicitacoes", r);
    public Task<List<Solicitacao>> GetMinhasSolicitacoesAsync() => ListAsync<Solicitacao>("api/meus/solicitacoes");

    public async Task<Associado?> GetPerfilAsync()
    {
        try { return await http.GetFromJsonAsync<Associado>("api/meus/perfil"); }
        catch (HttpRequestException) { return null; }
    }

    public Task<(bool ok, string msg)> AddDependenteAsync(Dependente d) => PostAsync("api/meus/dependentes", d);

    public Task<(bool ok, string msg)> CriarAcessoDependenteAsync(int dependenteId, string email, string senha) =>
        PostAsync($"api/meus/dependentes/{dependenteId}/acesso", new AcessoDependenteRequest(email, senha));

    public async Task<(bool ok, string msg)> RemoverDependenteAsync(int id)
    {
        try
        {
            var r = await http.DeleteAsync($"api/meus/dependentes/{id}");
            return r.IsSuccessStatusCode ? (true, "") : (false, await LerErro(r));
        }
        catch (HttpRequestException) { return (false, SemConexao); }
    }

    async Task<(bool ok, string msg)> PostAsync<T>(string url, T body)
    {
        try
        {
            var r = await http.PostAsJsonAsync(url, body);
            return r.IsSuccessStatusCode ? (true, "") : (false, await LerErro(r));
        }
        catch (HttpRequestException) { return (false, SemConexao); }
    }

    // Lista vazia quando offline ou quando o servidor recusa (ex.: cadastro ainda pendente).
    async Task<List<T>> ListAsync<T>(string url)
    {
        try { return await http.GetFromJsonAsync<List<T>>(url) ?? new(); }
        catch (HttpRequestException) { return new(); }
    }

    static async Task<string> LerErro(HttpResponseMessage r)
    {
        try
        {
            var e = await r.Content.ReadFromJsonAsync<Erro>();
            if (!string.IsNullOrWhiteSpace(e?.Mensagem)) return e.Mensagem;
        }
        catch (Exception) { /* corpo não era JSON */ }
        return r.StatusCode == HttpStatusCode.Unauthorized ? "Faça login novamente." : $"Erro {(int)r.StatusCode} no servidor.";
    }
}
