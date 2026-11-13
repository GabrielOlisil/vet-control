using System;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace Api.Database.Configuration;

public class VacinaConfiguration : IEntityTypeConfiguration<Vacina>
{
    public void Configure(EntityTypeBuilder<Vacina> builder)
    {
        builder.Property(v => v.Ativo).HasDefaultValue(true);

        builder
        .HasOne(e => e.Especie).WithMany(e => e.VacinasRestritas).HasForeignKey(e => e.EspecieId)
        .OnDelete(DeleteBehavior.ClientNoAction);

        builder.HasIndex(b => new { b.Name })
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");
    }
}
