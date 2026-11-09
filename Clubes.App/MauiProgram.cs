using Clubes.App.Services;
using Clubes.App.ViewModels;
using Clubes.App.Views;
namespace Clubes.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var b = MauiApp.CreateBuilder();
        b.UseMauiApp<App>();

        b.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(ApiConfig.BaseUrl), Timeout = TimeSpan.FromSeconds(15) });
        b.Services.AddSingleton<AppState>();
        b.Services.AddSingleton<IClubeService, ClubeServiceHttp>();

        b.Services.AddTransient<ClubesViewModel>();
        b.Services.AddTransient<LoginViewModel>();
        b.Services.AddTransient<CadastroViewModel>();
        b.Services.AddTransient<EventosViewModel>();
        b.Services.AddTransient<MensalidadesViewModel>();
        b.Services.AddTransient<ReservasViewModel>();
        b.Services.AddTransient<SolicitacoesViewModel>();
        b.Services.AddTransient<ContaViewModel>();

        b.Services.AddTransient<ClubesPage>();
        b.Services.AddTransient<LoginPage>();
        b.Services.AddTransient<CadastroPage>();
        b.Services.AddTransient<EventosPage>();
        b.Services.AddTransient<MensalidadesPage>();
        b.Services.AddTransient<ReservasPage>();
        b.Services.AddTransient<SolicitacoesPage>();
        b.Services.AddTransient<ContaPage>();
        return b.Build();
    }
}
