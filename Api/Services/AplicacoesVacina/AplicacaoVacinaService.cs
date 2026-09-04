using Api.Database;
using Api.DTOs.AplicacoesVacina;
using Api.DTOs;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.AplicacoesVacina;

public sealed class AplicacaoVacinaService(ApiContext context) : IAplicacaoVacinaService
{
    /* TODO:    criar query smartfilter com filtros de data, vacina e cartao
                para retornar apenas as aplicacoes que interessam
                ao usuario, e nao todas as aplicacoes do banco.
     */


    public async Task<AplicacaoVacina> CreateAsync(AplicacaoVacinaCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var aplicacao = new AplicacaoVacina
        {
            Id = Guid.NewGuid(),
            VacinaId = dto.VacinaId,
            CartaoVacinaId = dto.CartaoVacinaId,
            DataAplicacao = dto.DataAplicacao
        };

        context.AplicacoesVacina.Add(aplicacao);
        await context.SaveChangesAsync(cancellationToken);

        return aplicacao;
    }

    public async Task<AplicacaoVacina?> PatchAsync(Guid id, AplicacaoVacinaPatchDto dto,
        CancellationToken cancellationToken = default)
    {
        var aplicacao =
            await context.AplicacoesVacina.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (aplicacao is null)
        {
            return null;
        }

        if (dto.VacinaId.HasValue)
        {
            aplicacao.VacinaId = dto.VacinaId.Value;
        }

        if (dto.DataAplicacao.HasValue)
        {
            aplicacao.DataAplicacao = dto.DataAplicacao.Value;
        }

        if (dto.CartaoVacinaId.HasValue)
        {
            aplicacao.CartaoVacinaId = dto.CartaoVacinaId;
        }

        await context.SaveChangesAsync(cancellationToken);

        return aplicacao;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var aplicacao =
            await context.AplicacoesVacina.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (aplicacao is null)
        {
            return false;
        }

        context.AplicacoesVacina.Remove(aplicacao);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<AplicacaoVacina?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.AplicacoesVacina
            .AsNoTracking()
            .Include(aplicacao => aplicacao.Vacina)
            .FirstOrDefaultAsync(aplicacao => aplicacao.Id == id, cancellationToken);
    }

    public Task<List<AplicacaoVacina>> GetAllAsync(AplicacaoVacinaSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.AplicacoesVacina
            .AsNoTracking();

        if (search?.VacinaId is not null)
            query = query.Where(aplicacao => aplicacao.VacinaId == search.VacinaId);
        if (search?.CartaoVacinaId is not null)
            query = query.Where(aplicacao => aplicacao.CartaoVacinaId == search.CartaoVacinaId);
        if (search?.DataAplicacaoFrom is not null)
            query = query.Where(aplicacao => aplicacao.DataAplicacao >= search.DataAplicacaoFrom);
        if (search?.DataAplicacaoTo is not null)
            query = query.Where(aplicacao => aplicacao.DataAplicacao <= search.DataAplicacaoTo);

        return query.OrderByDescending(aplicacao => aplicacao.DataAplicacao)
            .ToListAsync(cancellationToken);
    }
}