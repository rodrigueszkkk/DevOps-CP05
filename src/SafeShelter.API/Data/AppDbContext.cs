using Microsoft.EntityFrameworkCore;
using SafeShelter.Api.Models;

namespace SafeShelter.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Comunidade> Comunidades => Set<Comunidade>();
    public DbSet<Dispositivo> Dispositivos => Set<Dispositivo>();
    public DbSet<SosLog> SosLogs => Set<SosLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Comunidade>(entity =>
        {
            entity.ToTable("Comunidades");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nome).IsRequired().HasMaxLength(150);
            entity.Property(c => c.PoligonoGeografico).HasMaxLength(500);
            entity.Property(c => c.CriadoEm).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Dispositivo>(entity =>
        {
            entity.ToTable("Dispositivos");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.MacAddress).IsRequired().HasMaxLength(50);
            entity.Property(d => d.PerfilResponsavel).IsRequired().HasMaxLength(100);
            entity.Property(d => d.CriadoEm).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(d => d.Comunidade)
                  .WithMany(c => c.Dispositivos)
                  .HasForeignKey(d => d.ComunidadeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SosLog>(entity =>
        {
            entity.ToTable("SosLogs");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.TipoAlerta).IsRequired().HasMaxLength(80);
            entity.Property(s => s.Status).IsRequired().HasMaxLength(40);
            entity.Property(s => s.Timestamp).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(s => s.Dispositivo)
                  .WithMany(d => d.SosLogs)
                  .HasForeignKey(s => s.DispositivoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
