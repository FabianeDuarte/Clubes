using System.Collections.ObjectModel;
using Clubes.App.Services;
using Clubes.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace Clubes.App.ViewModels;

public partial class SolicitacoesViewModel(IClubeService svc, AppState state) : ObservableObject
{
    public ObservableCollection<Solicitacao> Itens { get; } = new();
    public Array Tipos { get; } = Enum.GetValues<TipoSolicitacao>();
    [ObservableProperty] TipoSolicitacao tipo = TipoSolicitacao.Solicitacao;
    [ObservableProperty] string assunto = "";
    [ObservableProperty] string texto = "";
    [ObservableProperty] string mensagem = "";

    [RelayCommand]
    async Task CarregarAsync()
    {
        Itens.Clear();
        if (!state.Logado) return;
        foreach (var s in await svc.GetMinhasSolicitacoesAsync()) Itens.Add(s);
    }

    [RelayCommand]
    async Task EnviarAsync()
    {
        if (!state.Logado) { Mensagem = "Entre na sua conta para enviar."; return; }
        var (ok, msg) = await svc.EnviarSolicitacaoAsync(new SolicitacaoRequest(Tipo, Assunto, Texto));
        Mensagem = ok ? "Enviado ao clube." : msg;
        if (ok) { Assunto = Texto = ""; await CarregarAsync(); }
    }
}
