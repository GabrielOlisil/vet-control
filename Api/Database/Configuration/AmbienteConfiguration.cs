using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public sealed class AmbienteConfiguration : IEntityTypeConfiguration<Ambiente>
{
    public void Configure(EntityTypeBuilder<Ambiente> builder)
    {
        builder.Property(a => a.Nome).IsRequired();
        builder.Property(a => a.NomeNormalizado).HasComputedColumnSql("""lower(btrim("Nome"))""", stored: true);
        builder.Property(a => a.Tipo).HasConversion<string>();
        builder.HasIndex(a => a.NomeNormalizado).IsUnique();
        builder.HasIndex(a => a.Nome).HasMethod("GIN").HasOperators("gin_trgm_ops");
    }
}
