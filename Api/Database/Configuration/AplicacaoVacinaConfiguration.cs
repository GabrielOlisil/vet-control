
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public class AplicacaoVacinaConfiguration : IEntityTypeConfiguration<AplicacaoVacina>
{
    public void Configure(EntityTypeBuilder<AplicacaoVacina> builder)
    {
        builder.Property(a => a.Version).IsRowVersion();
        builder.Property(a => a.VersaoAssinatura).HasDefaultValue(1);
        builder.Property(a => a.SituacaoRegistro).HasConversion<string>().HasDefaultValue(Api.Models.Enums.SituacaoRegistroVacina.Ativo);
        builder.Property(a => a.SnapshotAssinado).HasColumnType("text");
        builder.Property(a => a.MotivoAlteracao).HasMaxLength(1000);
        builder.HasOne(a => a.AplicacaoOriginal).WithOne(a => a.SubstituidaPor)
            .HasForeignKey<AplicacaoVacina>(a => a.RetificaAplicacaoVacinaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(a => a.SituacaoRegistro);
        builder.HasIndex(a => new { a.AnimalId, a.VacinaId, a.SituacaoRegistro, a.DataAplicacao });
        builder.HasIndex(a => new { a.AnimalProtocoloVacinalId, a.ProtocoloVacinalDoseId, a.SituacaoRegistro, a.DataAplicacao });
        builder
            .HasOne(e => e.Animal)
            .WithMany(e => e.VacinasAplicadas)
            .HasForeignKey(animal => animal.AnimalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Vacina)
            .WithMany(e => e.Aplicacoes)
            .HasForeignKey(e => e.VacinaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AnimalProtocoloVacinal)
            .WithMany(e => e.Aplicacoes)
            .HasForeignKey(e => e.AnimalProtocoloVacinalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProtocoloVacinalDose)
            .WithMany(e => e.Aplicacoes)
            .HasForeignKey(e => e.ProtocoloVacinalDoseId)
            .OnDelete(DeleteBehavior.Restrict);

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
