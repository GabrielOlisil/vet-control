using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Database;

public class ApiContext(DbContextOptions<ApiContext> options) : DbContext(options)
{
    public DbSet<Especie> Especies { get; set; }
    public DbSet<Raca> Racas { get; set; }
    public DbSet<Animal> Animals { get; set; }
    public DbSet<Vacina> Vacinas { get; set; }
    public DbSet<AplicacaoVacina> AplicacoesVacina { get; set; }
    public DbSet<CartaoVacina> CartoesVacina { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Raca>()
            .HasOne(raca => raca.Especie)
            .WithMany(especie => especie.Racas)
            .HasForeignKey(raca => raca.EspecieId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Animal>()
            .HasOne(animal => animal.Raca)
            .WithMany(raca => raca.Animais)
            .HasForeignKey(animal => animal.RacaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Animal>()
            .HasOne(animal => animal.CartaoVacina)
            .WithMany()
            .HasForeignKey(animal => animal.CartaoVacinaId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AplicacaoVacina>()
            .HasOne(aplicacao => aplicacao.Vacina)
            .WithMany()
            .HasForeignKey(aplicacao => aplicacao.VacinaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AplicacaoVacina>()
            .HasOne(aplicacao => aplicacao.CartaoVacina)
            .WithMany(cartao => cartao.VacinasAplicadas)
            .HasForeignKey(aplicacao => aplicacao.CartaoVacinaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
