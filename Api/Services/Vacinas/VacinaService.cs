using Api.Database;
using Api.DTOs.Vacinas;
using Api.DTOs;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Vacinas;

public sealed class VacinaService(ApiContext context) : IVacinaService
{
    public Task<List<Vacina>> GetAllAsync(VacinaSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Vacinas
            .AsNoTracking();

        if (search?.ReaplicarEmXDiasMin is not null)
            query = query.Where(vacina => vacina.ReaplicarEmXDias >= search.ReaplicarEmXDiasMin);
        if (search?.ReaplicarEmXDiasMax is not null)
            query = query.Where(vacina => vacina.ReaplicarEmXDias <= search.ReaplicarEmXDiasMax);

        return query.OrderBy(vacina => vacina.Name).ToListAsync(cancellationToken);
    }

    public Task<Vacina?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Vacinas
            .AsNoTracking()
            .Where(vacina => vacina.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Vacina> CreateAsync(VacinaCreateDto dto, CancellationToken cancellationToken = default)
    {
        var vacina = new Vacina
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            ReaplicarEmXDias = dto.ReaplicarEmXDias
        };

        context.Vacinas.Add(vacina);
        await context.SaveChangesAsync(cancellationToken);

        return vacina;
    }

    public async Task<Vacina?> PatchAsync(Guid id, VacinaPatchDto dto,
        CancellationToken cancellationToken = default)
    {
        var vacina = await context.Vacinas.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (vacina is null)
        {
            return null;
        }

        if (dto.Name is not null)
        {
            vacina.Name = dto.Name;
        }

        if (dto.ReaplicarEmXDias.HasValue)
        {
            vacina.ReaplicarEmXDias = dto.ReaplicarEmXDias.Value;
        }

        await context.SaveChangesAsync(cancellationToken);

        return vacina;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vacina = await context.Vacinas.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (vacina is null)
        {
            return false;
        }

        context.Vacinas.Remove(vacina);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<int> Count(CancellationToken cancellationToken = default)
    {
        return context.Vacinas.CountAsync(cancellationToken);
    }
}