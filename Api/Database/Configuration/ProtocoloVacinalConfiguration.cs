using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public sealed class ProtocoloVacinalConfiguration : IEntityTypeConfiguration<ProtocoloVacinal>
{
    public void Configure(EntityTypeBuilder<ProtocoloVacinal> builder)
    {
        builder.Property(p => p.Version).IsRowVersion();
        builder.Property(p => p.FamiliaId).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Versao).HasDefaultValue(1);
        builder.Property(p => p.VersaoAtual).HasDefaultValue(true);
        builder.HasIndex(p => new { p.FamiliaId, p.Versao }).IsUnique();
        builder.HasIndex(p => p.FamiliaId).IsUnique().HasFilter("\"VersaoAtual\"");
        builder.HasOne(e => e.Especie)
            .WithMany(e => e.ProtocolosVacinais)
            .HasForeignKey(e => e.EspecieId)
            .OnDelete(DeleteBehavior.ClientNoAction);

        builder.HasIndex(e => e.Nome)
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => new { e.EspecieId, e.Ativo });
    }
}
