using System.Collections.ObjectModel;
using Clubes.App.Services;
using Clubes.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace Clubes.App.ViewModels;

public partial class ContaViewModel(IClubeService svc, AppState state) : ObservableObject
{
    public ObservableCollection<Dependente> Dependentes { get; } = new();
    [ObservableProperty] string resumo = "";
    [ObservableProperty] string nomeDep = "";
    [ObservableProperty] string parentesco = "";
    [ObservableProperty] DateTime nascDep = DateTime.Today.AddYears(-10);
    [ObservableProperty] bool temApp;
    [ObservableProperty] string acessoEmail = "";
    [ObservableProperty] string acessoSenha = "";
    [ObservableProperty] string mensagem = "";

    [RelayCommand]
    async Task CarregarAsync()
    {
        Dependentes.Clear();
        if (!state.Logado) { Resumo = "Você não está logado. Use a aba Entrar."; return; }
        var p = await svc.GetPerfilAsync();
        if (p is null) { Resumo = $"Olá, {state.Sessao!.Nome}."; return; }
        Resumo = $"{p.Nome} — {p.Tipo}\nSituação do cadastro: {p.Aprovacao}\nDireito a voto: {(p.TemDireitoVoto ? "sim" : "não")}";
        foreach (var d in p.Dependentes) Dependentes.Add(d);
    }

    [RelayCommand]
    async Task AddDependenteAsync()
    {
        var (ok, msg) = await svc.AddDependenteAsync(new Dependente { Nome = NomeDep, Parentesco = Parentesco, DataNascimento = NascDep, TemAplicativo = TemApp });
        Mensagem = ok ? "Dependente adicionado." : msg;
        if (ok) { NomeDep = Parentesco = ""; TemApp = false; await CarregarAsync(); }
    }

    [RelayCommand]
    async Task RemoverAsync(Dependente d)
    {
        var (ok, msg) = await svc.RemoverDependenteAsync(d.Id);
        Mensagem = ok ? "Dependente removido." : msg;
        if (ok) await CarregarAsync();
    }

    [RelayCommand]
    async Task CriarAcessoAsync(Dependente d)
    {
        var (ok, msg) = await svc.CriarAcessoDependenteAsync(d.Id, AcessoEmail, AcessoSenha);
        Mensagem = ok ? $"Acesso criado para {d.Nome}." : msg;
        if (ok) { AcessoEmail = AcessoSenha = ""; await CarregarAsync(); }
    }

    [RelayCommand]
    async Task SairAsync()
    {
        svc.Sair();
        await Shell.Current.GoToAsync("//Entrar");
    }
}
