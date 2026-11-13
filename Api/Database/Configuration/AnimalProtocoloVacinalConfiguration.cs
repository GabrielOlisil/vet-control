using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public sealed class AnimalProtocoloVacinalConfiguration : IEntityTypeConfiguration<AnimalProtocoloVacinal>
{
    public void Configure(EntityTypeBuilder<AnimalProtocoloVacinal> builder)
    {
        builder.HasOne(e => e.Animal)
            .WithMany(e => e.ProtocolosVacinais)
            .HasForeignKey(e => e.AnimalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProtocoloVacinal)
            .WithMany(e => e.AplicacoesEmAnimais)
            .HasForeignKey(e => e.ProtocoloVacinalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.AnimalId, e.Ativo });
        builder.HasIndex(e => new { e.AnimalId, e.ProtocoloVacinalId }).IsUnique().HasFilter("\"Ativo\"");
        builder.HasIndex(e => e.ProtocoloVacinalId);
        builder.HasIndex(e => e.DataInicio);
    }
}
