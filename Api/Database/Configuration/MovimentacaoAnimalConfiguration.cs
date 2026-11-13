using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public sealed class MovimentacaoAnimalConfiguration : IEntityTypeConfiguration<MovimentacaoAnimal>
{
    public void Configure(EntityTypeBuilder<MovimentacaoAnimal> builder)
    {
        builder.HasOne<Ambiente>().WithMany().HasForeignKey(m => m.AmbienteOrigemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Ambiente>().WithMany().HasForeignKey(m => m.AmbienteDestinoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Animal)
            .WithMany(e => e.Movimentacoes)
            .HasForeignKey(e => e.AnimalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.AnimalId, e.DataMovimentacao });
        builder.HasIndex(e => e.LocalDestino)
            .HasMethod("GIN")
            .HasOperators("gin_trgm_ops");
    }
}
