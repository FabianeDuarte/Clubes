using Clubes.Shared;
namespace Clubes.App.ViewModels;

public class AppState
{
    public Clube? ClubeAtual { get; set; }
    public LoginResponse? Sessao { get; set; }
    public bool Logado => Sessao is not null;
}
