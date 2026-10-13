
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

        builder.HasOne(e => e.AnimalProtocoloVacinal)
            .WithMany(e => e.Aplicacoes)
            .HasForeignKey(e => e.AnimalProtocoloVacinalId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ProtocoloVacinalDose)
            .WithMany(e => e.Aplicacoes)
            .HasForeignKey(e => e.ProtocoloVacinalDoseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.DataAplicacao);
        builder.HasIndex(a => a.DataProximaDose);
        builder.HasIndex(a => a.StatusComprovante);
        builder.HasIndex(a => a.HashAssinatura);

        builder.Property(a => a.VeterinarioCrmv).HasMaxLength(50);
        builder.Property(a => a.VeterinarioCrmvUf).HasMaxLength(2);
        builder.Property(a => a.AssinanteCrmv).HasMaxLength(50);
        builder.Property(a => a.AssinanteCrmvUf).HasMaxLength(2);
        builder.Property(a => a.HashAssinatura).HasMaxLength(64);
        builder.Property(a => a.MetodoAssinatura).HasMaxLength(50);
    }
}
