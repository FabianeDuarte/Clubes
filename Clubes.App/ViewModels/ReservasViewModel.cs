using System.Collections.ObjectModel;
using Clubes.App.Services;
using Clubes.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace Clubes.App.ViewModels;

public partial class ReservasViewModel(IClubeService svc, AppState state) : ObservableObject
{
    public ObservableCollection<AgendaItem> Agenda { get; } = new();
    public ObservableCollection<ReservaSalao> Minhas { get; } = new();
    public Array Finalidades { get; } = Enum.GetValues<FinalidadeReserva>();

    [ObservableProperty] FinalidadeReserva finalidade = FinalidadeReserva.Churrasco;
    [ObservableProperty] string descricao = "";
    [ObservableProperty] DateTime dataInicio = DateTime.Today.AddDays(7);
    [ObservableProperty] TimeSpan horaInicio = new(11, 0, 0);
    [ObservableProperty] DateTime dataFim = DateTime.Today.AddDays(7);
    [ObservableProperty] TimeSpan horaFim = new(17, 0, 0);
    [ObservableProperty] string mensagem = "";

    [RelayCommand]
    async Task CarregarAsync()
    {
        Agenda.Clear(); Minhas.Clear();
        if (state.ClubeAtual is null) { Mensagem = "Escolha um clube na aba Clubes."; return; }
        foreach (var a in await svc.GetAgendaAsync(state.ClubeAtual.Id)) Agenda.Add(a);
        if (state.Logado) foreach (var r in await svc.GetMinhasReservasAsync()) Minhas.Add(r);
    }

    [RelayCommand]
    async Task ReservarAsync()
    {
        if (!state.Logado) { Mensagem = "Entre na sua conta para reservar."; return; }
        var (ok, msg) = await svc.SolicitarReservaAsync(new ReservaRequest(Finalidade, Descricao,
            DataInicio.Date + HoraInicio, DataFim.Date + HoraFim));
        Mensagem = ok ? "Reserva enviada. Aguarde a aprovação do clube." : msg;
        if (ok) { Descricao = ""; await CarregarAsync(); }
    }
}
