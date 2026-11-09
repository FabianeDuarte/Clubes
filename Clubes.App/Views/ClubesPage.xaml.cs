using System.Windows.Input;
using Clubes.App.ViewModels;
namespace Clubes.App.Views;

public partial class ClubesPage : ContentPage
{
    public ClubesPage(ClubesViewModel vm) { InitializeComponent(); BindingContext = vm; }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var cmd = BindingContext?.GetType().GetProperty("CarregarCommand")?.GetValue(BindingContext) as ICommand;
        cmd?.Execute(null);
    }
}
