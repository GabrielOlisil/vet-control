using System;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace Api.Database.Configuration;

public class EspecieConfiguration : IEntityTypeConfiguration<Especie>
{
    public void Configure(EntityTypeBuilder<Especie> builder)
    {
        builder.HasIndex(b => new { b.Nome })
             .HasMethod("GIN")
             .HasOperators("gin_trgm_ops");

        builder.HasIndex(b => new { b.NomeCientifico })
        .HasMethod("GIN")
        .HasOperators("gin_trgm_ops");
    }
}
