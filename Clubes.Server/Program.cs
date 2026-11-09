using Clubes.Server;
using Clubes.Server.Api;
using Clubes.Server.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var b = WebApplication.CreateBuilder(args);

b.Services.AddDbContext<ClubesDb>(o => o.UseSqlite(b.Configuration.GetConnectionString("Db")));
b.Services.AddSingleton<TokenService>();

b.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => { o.LoginPath = "/Login"; o.AccessDeniedPath = "/Login"; o.ExpireTimeSpan = TimeSpan.FromHours(8); })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = TokenService.Parametros(b.Configuration);
    });

const string Super = "SuperAdmin", Pres = "PresidenteAdmin", Tes = "Tesoureiro";
b.Services.AddAuthorization(o =>
{
    o.AddPolicy("Api", p => p.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser());
    o.AddPolicy("Painel", p => p.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme).RequireRole(Super, Pres, Tes));
    o.AddPolicy("Gestao", p => p.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme).RequireRole(Super, Pres));
    o.AddPolicy("Super",  p => p.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme).RequireRole(Super));
});

b.Services.AddRazorPages(o =>
{
    o.Conventions.AuthorizeFolder("/Admin", "Painel");                 // presidente, tesoureiro e super
    o.Conventions.AuthorizePage("/Admin/Clubes", "Super");             // só super admin
    foreach (var p in new[] { "Associados", "AssociadoEditar", "Usuarios", "Reservas", "Eventos", "Solicitacoes" })
        o.Conventions.AuthorizePage($"/Admin/{p}", "Gestao");          // presidente e super
    // Admin/Mensalidades e Admin/Index: os 3 perfis
});

var app = b.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClubesDb>();
    db.Database.EnsureCreated();
    Seed.Run(db);
}

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapApi();
app.Run();
