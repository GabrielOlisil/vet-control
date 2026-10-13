using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public sealed class ProtocoloVacinalConfiguration : IEntityTypeConfiguration<ProtocoloVacinal>
{
    public void Configure(EntityTypeBuilder<ProtocoloVacinal> builder)
    {
        builder.HasOne(e => e.Especie)
            .WithMany(e => e.ProtocolosVacinais)
            .HasForeignKey(e => e.EspecieId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.Nome)
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex(e => new { e.EspecieId, e.Ativo });
    }
}
