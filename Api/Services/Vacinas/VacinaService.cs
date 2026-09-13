using Api.Database;
using Api.DTOs;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Vacinas;

public sealed class VacinaService(ApiContext context) : IVacinaService
{
    public Task<List<Vacina>> GetAllAsync(int? page, VacinaSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Vacinas
            .Include(v => v.Especie)
            .AsNoTracking();

        if (search?.EspecieId is not null)
            query = query.Where(vacina => vacina.EspecieId == search.EspecieId);
        if (search?.ObrigatorioOrgaoSanitario is not null)
            query = query.Where(vacina => vacina.ObrigatorioOrgaoSanitario == search.ObrigatorioOrgaoSanitario);
        if (search?.ReaplicarEmXDiasMin is not null)
            query = query.Where(vacina => vacina.ReaplicarEmXDias >= search.ReaplicarEmXDiasMin);
        if (search?.ReaplicarEmXDiasMax is not null)
            query = query.Where(vacina => vacina.ReaplicarEmXDias <= search.ReaplicarEmXDiasMax);

        query = query.OrderBy(vacina => vacina.Name);

        if (page.HasValue)
        {
            query = query.Skip((page.Value - 1) * 10).Take(10);
        }

        return query.ToListAsync(cancellationToken);
    }

    public Task<Vacina?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Vacinas
            .AsNoTracking()
            .Include(v => v.Especie)
            .Include(v => v.Aplicacoes)
            .Where(vacina => vacina.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Vacina> CreateAsync(VacinaCreateDto dto, CancellationToken cancellationToken = default)
    {
        var vacina = new Vacina
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Descricao = dto.Descricao,
            ReaplicarEmXDias = dto.ReaplicarEmXDias,
            ObrigatorioOrgaoSanitario = dto.ObrigatorioOrgaoSanitario,
            EspecieId = dto.EspecieId
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

        if (dto.Descricao is not null)
        {
            vacina.Descricao = dto.Descricao;
        }

        if (dto.ReaplicarEmXDias.HasValue)
        {
            vacina.ReaplicarEmXDias = dto.ReaplicarEmXDias.Value;
        }

        if (dto.ObrigatorioOrgaoSanitario.HasValue)
        {
            vacina.ObrigatorioOrgaoSanitario = dto.ObrigatorioOrgaoSanitario.Value;
        }

        if (dto.EspecieId.HasValue)
        {
            vacina.EspecieId = dto.EspecieId.Value;
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

    public Task<int> Count(VacinaSearchDto? search = null, CancellationToken cancellationToken = default)
    {
        var query = context.Vacinas.AsNoTracking();

        if (search?.EspecieId is not null)
            query = query.Where(vacina => vacina.EspecieId == search.EspecieId);
        if (search?.ObrigatorioOrgaoSanitario is not null)
            query = query.Where(vacina => vacina.ObrigatorioOrgaoSanitario == search.ObrigatorioOrgaoSanitario);
        if (search?.ReaplicarEmXDiasMin is not null)
            query = query.Where(vacina => vacina.ReaplicarEmXDias >= search.ReaplicarEmXDiasMin);
        if (search?.ReaplicarEmXDiasMax is not null)
            query = query.Where(vacina => vacina.ReaplicarEmXDias <= search.ReaplicarEmXDiasMax);

        return query.CountAsync(cancellationToken);
    }

    public Task<List<Vacina>> GetAllByNameAsync(int page, string name, CancellationToken cancellationToken = default)
    {
        var query = context.Vacinas.AsNoTracking()
             .Where(e => EF.Functions.ILike(e.Name, $"{name}%"))
             .OrderBy(e => e.Name)
                .Skip((page - 1) * 10)
                .Take(10);

        return query.ToListAsync(cancellationToken);
    }
}