using Api.Database;
using Api.DTOs.Racas;
using Api.DTOs;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Racas;

public sealed class RacaService(ApiContext context) : IRacaService
{
    /* public Task<List<Raca>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return context.Racas
            .AsNoTracking()
            .OrderBy(r => r.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Raca?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Racas
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    } */

    public async Task<Raca> CreateAsync(RacaCreateDto dto, CancellationToken cancellationToken = default)
    {
        var raca = new Raca
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            EspecieId = dto.EspecieId
        };
        context.Racas.Add(raca);
        await context.SaveChangesAsync(cancellationToken);
        return raca;
    }

    public async Task<Raca?> PatchAsync(Guid id, RacaPatchDto dto,
        CancellationToken cancellationToken = default)
    {
        var raca = await context.Racas
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (raca is null)
        {
            return null;
        }

        if (dto.Nome is not null)
        {
            raca.Nome = dto.Nome;
        }


        await context.SaveChangesAsync(cancellationToken);

        return raca;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var raca = await context.Racas
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (raca is null)
        {
            return false;
        }

        context.Racas.Remove(raca);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }


    public Task<Raca?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Racas
            .AsNoTracking()
            .Include(r => r.Especie)
            .Include(r => r.Animais)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public Task<List<Raca>> GetAllAsync(RacaSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Racas
            .AsNoTracking();

        if (search?.EspecieId is not null)
            query = query.Where(raca => raca.EspecieId == search.EspecieId);

        return query.OrderBy(r => r.Nome).ToListAsync(cancellationToken);
    }
}
