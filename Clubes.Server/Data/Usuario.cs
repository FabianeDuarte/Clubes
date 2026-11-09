using Clubes.Shared;
using Microsoft.AspNetCore.Identity;
namespace Clubes.Server.Data;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";      // sempre minúsculo
    public string SenhaHash { get; set; } = "";
    public PerfilUsuario Perfil { get; set; }
    public int? ClubeId { get; set; }            // null para SuperAdmin
    public int? AssociadoId { get; set; }        // associado (ou titular, no caso de dependente)
    public int? DependenteId { get; set; }       // preenchido para perfil Dependente
}

public static class Senhas
{
    static readonly PasswordHasher<Usuario> H = new();
    public static string Hash(Usuario u, string senha) => H.HashPassword(u, senha);
    public static bool Confere(Usuario u, string senha) =>
        H.VerifyHashedPassword(u, u.SenhaHash, senha) != PasswordVerificationResult.Failed;
}
