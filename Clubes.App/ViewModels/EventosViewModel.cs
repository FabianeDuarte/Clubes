using System.Collections.ObjectModel;
using Clubes.App.Services;
using Clubes.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace Clubes.App.ViewModels;

public partial class EventosViewModel(IClubeService svc, AppState state) : ObservableObject
{
    public ObservableCollection<Evento> Itens { get; } = new();
    [ObservableProperty] string mensagem = "";

    [RelayCommand]
    async Task CarregarAsync()
    {
        Itens.Clear();
        if (state.ClubeAtual is null) { Mensagem = "Escolha um clube na aba Clubes."; return; }
        foreach (var e in await svc.GetEventosAsync(state.ClubeAtual.Id)) Itens.Add(e);
        Mensagem = Itens.Count == 0 ? "Nenhum evento programado." : "";
    }
}
