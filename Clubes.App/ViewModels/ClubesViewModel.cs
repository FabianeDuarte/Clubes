using System.Collections.ObjectModel;
using Clubes.App.Services;
using Clubes.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace Clubes.App.ViewModels;

public partial class ClubesViewModel(IClubeService svc, AppState state) : ObservableObject
{
    public ObservableCollection<Clube> Clubes { get; } = new();
    [ObservableProperty] Clube? selecionado;
    [ObservableProperty] string mensagem = "";

    [RelayCommand]
    async Task CarregarAsync()
    {
        Clubes.Clear();
        foreach (var c in await svc.GetClubesAsync()) Clubes.Add(c);
        Mensagem = Clubes.Count == 0 ? "Sem conexão com o servidor." : "";
        Selecionado = Clubes.FirstOrDefault(c => c.Id == state.ClubeAtual?.Id);
    }

    partial void OnSelecionadoChanged(Clube? value) { if (value is not null) state.ClubeAtual = value; }
}
