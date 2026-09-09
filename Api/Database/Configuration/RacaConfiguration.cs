
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace Api.Database.Configuration;

public class RacaConfiguration : IEntityTypeConfiguration<Raca>
{
    public void Configure(EntityTypeBuilder<Raca> builder)
    {
        builder
        .HasOne(raca => raca.Especie)
        .WithMany(especie => especie.Racas)
        .HasForeignKey(raca => raca.EspecieId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => new { b.Nome })
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");
    }
}
