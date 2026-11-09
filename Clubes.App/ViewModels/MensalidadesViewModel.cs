using System.Collections.ObjectModel;
using Clubes.App.Services;
using Clubes.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace Clubes.App.ViewModels;

public partial class MensalidadesViewModel(IClubeService svc, AppState state) : ObservableObject
{
    public ObservableCollection<Mensalidade> Itens { get; } = new();
    [ObservableProperty] string resumo = "";

    [RelayCommand]
    async Task CarregarAsync()
    {
        Itens.Clear();
        if (!state.Logado) { Resumo = "Entre na sua conta para ver as mensalidades."; return; }
        foreach (var m in await svc.GetMensalidadesAsync()) Itens.Add(m);
        if (Itens.Count == 0) { Resumo = "Nenhuma mensalidade (cadastro pendente de aprovação ou sem cobranças)."; return; }
        var atrasadas = Itens.Count(m => m.Situacao == SituacaoMensalidade.Atrasada);
        Resumo = atrasadas == 0 ? "Situação financeira: em dia" : $"Situação financeira: {atrasadas} mensalidade(s) em atraso";
    }
}
