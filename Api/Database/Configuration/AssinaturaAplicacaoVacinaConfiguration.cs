using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public class AssinaturaAplicacaoVacinaConfiguration : IEntityTypeConfiguration<AssinaturaAplicacaoVacina>
{
    public void Configure(EntityTypeBuilder<AssinaturaAplicacaoVacina> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.NomeAssinante).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Crmv).HasMaxLength(50).IsRequired();
        builder.Property(e => e.CrmvUf).HasMaxLength(2).IsRequired();
        builder.Property(e => e.TipoEvento).HasMaxLength(50).IsRequired();
        builder.Property(e => e.HashConteudo).HasMaxLength(64).IsRequired();
        builder.Property(e => e.Metodo).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Observacoes).HasMaxLength(1000);

        builder.HasOne(e => e.AplicacaoVacina)
            .WithMany(e => e.Assinaturas)
            .HasForeignKey(e => e.AplicacaoVacinaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.AplicacaoVacinaId);
        builder.HasIndex(e => e.DataHora);
    }
}
