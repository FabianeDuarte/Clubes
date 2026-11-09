using Clubes.Shared;
using Microsoft.EntityFrameworkCore;
namespace Clubes.Server.Data;

public class ClubesDb(DbContextOptions<ClubesDb> options) : DbContext(options)
{
    public DbSet<Clube> Clubes => Set<Clube>();
    public DbSet<Associado> Associados => Set<Associado>();
    public DbSet<Dependente> Dependentes => Set<Dependente>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Mensalidade> Mensalidades => Set<Mensalidade>();
    public DbSet<ReservaSalao> Reservas => Set<ReservaSalao>();
    public DbSet<Solicitacao> Solicitacoes => Set<Solicitacao>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
        b.Entity<Associado>().HasIndex(a => new { a.ClubeId, a.CPF }).IsUnique();
        b.Entity<Associado>().HasMany(a => a.Dependentes).WithOne().HasForeignKey(d => d.AssociadoId);
        b.Entity<Mensalidade>().HasIndex(m => new { m.AssociadoId, m.Competencia }).IsUnique();
        b.Entity<Clube>().Property(c => c.ValorMensalidade).HasPrecision(10, 2);
        b.Entity<Mensalidade>().Property(m => m.Valor).HasPrecision(10, 2);
    }
}
