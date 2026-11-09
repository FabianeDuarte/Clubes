using System.Security.Claims;
namespace Clubes.Server;

public static class Contas
{
    public static bool EhSuper(this ClaimsPrincipal u) => u.IsInRole("SuperAdmin");
    // null = todos os clubes (super admin); senão, o clube do usuário.
    public static int? ClubeFiltro(this ClaimsPrincipal u) =>
        u.EhSuper() ? null : int.Parse(u.FindFirstValue("clubeId")!);
}
