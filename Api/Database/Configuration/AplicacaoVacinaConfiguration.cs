
using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Configuration;

public class AplicacaoVacinaConfiguration : IEntityTypeConfiguration<AplicacaoVacina>
{
    public void Configure(EntityTypeBuilder<AplicacaoVacina> builder)
    {
        builder.HasOne(aplicacao => aplicacao.Vacina)
                    .WithMany()
                    .HasForeignKey(aplicacao => aplicacao.VacinaId)
                    .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(aplicacao => aplicacao.CartaoVacina)
                    .WithMany(cartao => cartao.VacinasAplicadas)
                    .HasForeignKey(aplicacao => aplicacao.CartaoVacinaId)
                    .OnDelete(DeleteBehavior.SetNull);
    }
}
