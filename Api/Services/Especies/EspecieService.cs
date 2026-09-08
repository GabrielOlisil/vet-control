using Api.Database;
using Api.DTOs.Especies;
using Api.DTOs;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Especies;

public sealed class EspecieService(ApiContext context) : IEspecieService
{
    public Task<List<Especie>> GetAllAsync(int? page, EspecieSearchDto? search = null, CancellationToken cancellationToken = default)
    {
        var query = context.Especies
            .AsNoTracking();

        if (search?.Nome is not null)
        {
            query = query.Where(e => e.Nome == search.Nome);
        }
        if (search?.NomeCientifico is not null)
        {
            query = query.Where(e => e.NomeCientifico == search.NomeCientifico);
        }

        if (!page.HasValue)
        {
            page = 1;
        }
        query = query.OrderBy(e => e.Nome).Skip((page.Value - 1) * 10).Take(10);

        return query.ToListAsync(cancellationToken);
    }

    public Task<Especie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Especies
            .AsNoTracking()
            .Include(e => e.Racas)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Especie> CreateAsync(EspecieCreateDto dto, CancellationToken cancellationToken = default)
    {
        var especie = new Especie
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome,
            NomeCientifico = dto.NomeCientifico
        };

        context.Especies.Add(especie);
        await context.SaveChangesAsync(cancellationToken);

        return especie;
    }

    public async Task<Especie?> PatchAsync(Guid id, EspeciePatchDto dto,
        CancellationToken cancellationToken = default)
    {
        var especie = await context.Especies
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (especie is null)
        {
            return null;
        }

        if (dto.Nome is not null)
        {
            especie.Nome = dto.Nome;
        }

        if (dto.NomeCientifico is not null)
        {
            especie.NomeCientifico = dto.NomeCientifico;
        }

        await context.SaveChangesAsync(cancellationToken);

        return especie;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var especie = await context.Especies
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (especie is null)
        {
            return false;
        }

        context.Especies.Remove(especie);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<int> Count(EspecieSearchDto? search = null, CancellationToken cancellationToken = default)
    {

        var query = context.Especies
          .AsNoTracking();

        if (search?.Nome is not null)
        {
            query = query.Where(e => e.Nome == search.Nome);
        }
        if (search?.NomeCientifico is not null)
        {
            query = query.Where(e => e.NomeCientifico == search.NomeCientifico);
        }

        return query.CountAsync(cancellationToken);
    }
}
