using Clubes.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace Clubes.App.ViewModels;

public partial class LoginViewModel(IClubeService svc, AppState state) : ObservableObject
{
    [ObservableProperty] string email = "";
    [ObservableProperty] string senha = "";
    [ObservableProperty] string mensagem = "";

    [RelayCommand]
    async Task EntrarAsync()
    {
        var (ok, msg) = await svc.LoginAsync(Email, Senha);
        if (!ok) { Mensagem = msg; return; }
        Senha = ""; Mensagem = "";
        var clubes = await svc.GetClubesAsync();
        state.ClubeAtual = clubes.FirstOrDefault(c => c.Id == state.Sessao?.ClubeId) ?? state.ClubeAtual;
        await Shell.Current.GoToAsync("//Conta");
    }
}
