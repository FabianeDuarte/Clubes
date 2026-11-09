# Execute na pasta onde estão Clubes.Shared, Clubes.Server e Clubes.App (requer .NET 10 SDK + workload MAUI).
$ErrorActionPreference = "Stop"

dotnet new sln -n Clubes --force
dotnet sln add Clubes.Shared\Clubes.Shared.csproj Clubes.Server\Clubes.Server.csproj

# Gera o esqueleto MAUI (Platforms, Resources, csproj...) e junta com o nosso código sem sobrescrevê-lo.
dotnet new maui -n Clubes.App -o _maui_tmp
robocopy _maui_tmp Clubes.App /E /XF AppShell.xaml AppShell.xaml.cs MauiProgram.cs MainPage.xaml MainPage.xaml.cs | Out-Null
Remove-Item _maui_tmp -Recurse -Force

dotnet add Clubes.App package CommunityToolkit.Mvvm
dotnet add Clubes.App reference Clubes.Shared\Clubes.Shared.csproj
dotnet sln add Clubes.App\Clubes.App.csproj

Write-Host "Pronto. Abra Clubes.sln no Visual Studio. Veja o LEIA-ME.md (cleartext no Android e execução)."
