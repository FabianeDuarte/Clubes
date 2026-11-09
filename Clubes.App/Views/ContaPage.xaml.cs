using System.Windows.Input;
using Clubes.App.ViewModels;
namespace Clubes.App.Views;

public partial class ContaPage : ContentPage
{
    public ContaPage(ContaViewModel vm) { InitializeComponent(); BindingContext = vm; }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var cmd = BindingContext?.GetType().GetProperty("CarregarCommand")?.GetValue(BindingContext) as ICommand;
        cmd?.Execute(null);
    }
}
