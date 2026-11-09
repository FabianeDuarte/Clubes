using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clubes.Server.Data;
using Microsoft.IdentityModel.Tokens;
namespace Clubes.Server.Api;

public class TokenService(IConfiguration cfg)
{
    public static TokenValidationParameters Parametros(IConfiguration c) => new()
    {
        ValidIssuer = "clubes", ValidAudience = "clubes-app",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(c["Jwt:Key"]!)),
        NameClaimType = "name", RoleClaimType = "role", ClockSkew = TimeSpan.FromMinutes(1)
    };

    public string Gerar(Usuario u)
    {
        var claims = new List<Claim> { new("sub", u.Id.ToString()), new("name", u.Nome), new("role", u.Perfil.ToString()) };
        if (u.ClubeId is int c) claims.Add(new("clubeId", c.ToString()));
        if (u.AssociadoId is int a) claims.Add(new("associadoId", a.ToString()));
        if (u.DependenteId is int d) claims.Add(new("dependenteId", d.ToString()));
        var cred = new SigningCredentials(Parametros(cfg).IssuerSigningKey, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken("clubes", "clubes-app", claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: cred);
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
