using System.Windows.Input;
using Clubes.App.ViewModels;
namespace Clubes.App.Views;

public partial class SolicitacoesPage : ContentPage
{
    public SolicitacoesPage(SolicitacoesViewModel vm) { InitializeComponent(); BindingContext = vm; }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var cmd = BindingContext?.GetType().GetProperty("CarregarCommand")?.GetValue(BindingContext) as ICommand;
        cmd?.Execute(null);
    }
}
