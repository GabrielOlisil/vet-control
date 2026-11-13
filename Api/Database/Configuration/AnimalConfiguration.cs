
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace Api.Database.Configuration;

public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        builder.Property(a => a.Version).IsRowVersion();
        builder.HasOne(a => a.Ambiente).WithMany(a => a.Animais)
            .HasForeignKey(a => a.AmbienteId).OnDelete(DeleteBehavior.Restrict);
        builder
        .HasOne(animal => animal.Raca)
        .WithMany(raca => raca.Animais)
        .HasForeignKey(animal => animal.RacaId)
        .OnDelete(DeleteBehavior.Restrict);


        builder.Property(a => a.Name).IsRequired(false);

        builder.Property(a => a.Sexo)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(b => new { b.Name })
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

    }
}
