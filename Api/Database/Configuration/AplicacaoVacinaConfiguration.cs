
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public class AplicacaoVacinaConfiguration : IEntityTypeConfiguration<AplicacaoVacina>
{
    public void Configure(EntityTypeBuilder<AplicacaoVacina> builder)
    {
        builder
            .HasOne(e => e.Animal)
            .WithMany(e => e.VacinasAplicadas)
            .HasForeignKey(animal => animal.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Vacina)
            .WithMany(e => e.Aplicacoes)
            .HasForeignKey(e => e.VacinaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.DataAplicacao);
        builder.HasIndex(a => a.DataProximaDose);
    }
}
