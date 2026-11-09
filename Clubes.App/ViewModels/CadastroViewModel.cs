using System.Collections.ObjectModel;
using Clubes.App.Services;
using Clubes.Shared;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace Clubes.App.ViewModels;

public partial class CadastroViewModel(IClubeService svc, AppState state) : ObservableObject
{
    public ObservableCollection<Clube> Clubes { get; } = new();
    public Array Tipos { get; } = Enum.GetValues<TipoAssociado>();
    public Array Sexos { get; } = Enum.GetValues<Sexo>();
    public Array EstadosCivis { get; } = Enum.GetValues<EstadoCivil>();

    [ObservableProperty] Clube? clube;
    [ObservableProperty] string nome = "";
    [ObservableProperty] string cpf = "";
    [ObservableProperty] string email = "";
    [ObservableProperty] string senha = "";
    [ObservableProperty] string fone = "";
    [ObservableProperty] string endereco = "";
    [ObservableProperty] string bairro = "";
    [ObservableProperty] string cidade = "";
    [ObservableProperty] string uf = "";
    [ObservableProperty] string interesses = "";   // separados por vírgula: pesca, caça, comida, bebida...
    [ObservableProperty] DateTime dataNascimento = DateTime.Today.AddYears(-30);
    [ObservableProperty] Sexo sexo;
    [ObservableProperty] EstadoCivil estadoCivil;
    [ObservableProperty] TipoAssociado tipo = TipoAssociado.Simpatizante;
    [ObservableProperty] string mensagem = "";

    [RelayCommand]
    async Task CarregarAsync()
    {
        if (Clubes.Count > 0) return;
        foreach (var c in await svc.GetClubesAsync()) Clubes.Add(c);
        Clube = Clubes.FirstOrDefault(c => c.Id == state.ClubeAtual?.Id) ?? Clubes.FirstOrDefault();
    }

    [RelayCommand]
    async Task SalvarAsync()
    {
        if (Clube is null) { Mensagem = "Escolha o clube."; return; }
        var lista = Interesses.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        var (ok, msg) = await svc.CadastrarAsync(new CadastroRequest(Clube.Id, Nome, Cpf, Email, Senha, Fone,
            Endereco, Bairro, Cidade, Uf, Sexo, EstadoCivil, DataNascimento, lista, Tipo));
        if (!ok) { Mensagem = msg; return; }
        Mensagem = "Cadastro enviado! Após a aprovação do clube você poderá usar todos os recursos. Entre na aba Entrar.";
        Senha = "";
    }
}
