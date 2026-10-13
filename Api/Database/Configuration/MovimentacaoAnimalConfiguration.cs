using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public sealed class MovimentacaoAnimalConfiguration : IEntityTypeConfiguration<MovimentacaoAnimal>
{
    public void Configure(EntityTypeBuilder<MovimentacaoAnimal> builder)
    {
        builder.HasOne(e => e.Animal)
            .WithMany(e => e.Movimentacoes)
            .HasForeignKey(e => e.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.AnimalId, e.DataMovimentacao });
        builder.HasIndex(e => e.LocalDestino)
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");
    }
}
