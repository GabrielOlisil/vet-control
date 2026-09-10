using Api.Models;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public class TipoIdentificadorConfiguration : IEntityTypeConfiguration<IdentificadorAnimal>
{
    public void Configure(EntityTypeBuilder<IdentificadorAnimal> builder)
    {
        builder
        .HasOne(e => e.Animal)
        .WithMany(e => e.Identificadores)
        .HasForeignKey(e => e.AnimalId)
        .OnDelete(DeleteBehavior.Cascade);


        builder.HasIndex(e => new { e.Valor, e.Tipo }).IsUnique();


        builder.HasIndex(b => b.Valor)
                .HasMethod("GIN")
                .HasOperators("gin_trgm_ops");
    }
}
