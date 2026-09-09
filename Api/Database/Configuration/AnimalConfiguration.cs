
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace Api.Database.Configuration;

public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        builder
        .HasOne(animal => animal.Raca)
        .WithMany(raca => raca.Animais)
        .HasForeignKey(animal => animal.RacaId)
        .OnDelete(DeleteBehavior.Restrict);

        builder
        .HasOne(animal => animal.CartaoVacina)
        .WithMany()
        .HasForeignKey(animal => animal.CartaoVacinaId)
        .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(b => new { b.Name })
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

    }
}
