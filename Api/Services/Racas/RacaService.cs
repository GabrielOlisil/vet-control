using Api.Database;
using Api.DTOs;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Racas;

public sealed class RacaService(ApiContext context) : IRacaService
{

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

        if (dto.EspecieId.HasValue)
        {
            raca.EspecieId = dto.EspecieId.Value;
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

    public Task<int> Count(RacaSearchDto? search = null, CancellationToken cancellationToken = default)
    {
        var query = context.Racas.AsNoTracking();
        query = ApplyFilters(query, search);
        return query.CountAsync(cancellationToken);
    }


    public Task<Raca?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Racas
            .AsNoTracking()
            .Include(r => r.Especie)
            .Include(r => r.Animais)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public Task<List<Raca>> GetAllAsync(int? page, RacaSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Racas.Include(e => e.Especie)
            .AsNoTracking();

        query = ApplyFilters(query, search);

        query = query.OrderBy(r => r.Nome);

        if (page.HasValue)
        {
            query = query.Skip((page.Value - 1) * 10).Take(10);
        }

        return query.ToListAsync(cancellationToken);
    }

    public Task<List<Raca>> GetAllByNameAsync(int page, string name, RacaSearchDto? search = null, CancellationToken cancellationToken = default)
    {
        var query = context.Racas
             .Include(e => e.Especie)
             .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(e => EF.Functions.ILike(e.Nome, $"{name}%"));
        }

        query = ApplyFilters(query, search);

        return query
             .OrderBy(e => e.Nome)
             .Skip((page - 1) * 10)
             .Take(10)
             .ToListAsync(cancellationToken);
    }

    private static IQueryable<Raca> ApplyFilters(IQueryable<Raca> query, RacaSearchDto? search)
    {
        if (search is null)
            return query;

        if (search.EspecieId.HasValue)
            query = query.Where(raca => raca.EspecieId == search.EspecieId.Value);

        return query;
    }
}
