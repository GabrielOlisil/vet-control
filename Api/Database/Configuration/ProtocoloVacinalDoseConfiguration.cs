using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public sealed class ProtocoloVacinalDoseConfiguration : IEntityTypeConfiguration<ProtocoloVacinalDose>
{
    public void Configure(EntityTypeBuilder<ProtocoloVacinalDose> builder)
    {
        builder.HasOne(e => e.ProtocoloVacinal)
            .WithMany(e => e.Doses)
            .HasForeignKey(e => e.ProtocoloVacinalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Vacina)
            .WithMany(e => e.DosesProtocolos)
            .HasForeignKey(e => e.VacinaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.ProtocoloVacinalId, e.Ordem }).IsUnique();
        builder.HasIndex(e => e.VacinaId);
    }
}
