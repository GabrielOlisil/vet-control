using Api.Database;
using Api.DTOs;
using Api.Models;
using Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Vacinas;

public sealed class VacinaService(ApiContext context) : IVacinaService
{
    public Task<List<Vacina>> GetAllAsync(int? page, VacinaSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Vacinas.Include(e => e.Especie)
            .AsNoTracking();

        query = ApplyFilters(query, search);

        query = query.OrderBy(vacina => vacina.Name);

        if (page.HasValue)
        {
            query = query.Skip(Pagination.Offset(page, 10)).Take(10);
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
            Ativo = dto.Ativo,
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Descricao = dto.Descricao,
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

        if (dto.Ativo.HasValue) vacina.Ativo = dto.Ativo.Value;
        if (dto.Name is not null)
        {
            vacina.Name = dto.Name;
        }

        if (dto.DescricaoWasProvided)
        {
            vacina.Descricao = dto.Descricao;
        }

        if (dto.ObrigatorioOrgaoSanitario.HasValue)
        {
            vacina.ObrigatorioOrgaoSanitario = dto.ObrigatorioOrgaoSanitario.Value;
        }

        if (dto.EspecieIdWasProvided)
        {
            vacina.EspecieId = dto.EspecieId;
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

        if (await context.AplicacoesVacina.AnyAsync(a => a.VacinaId == id, cancellationToken) ||
            await context.ProtocoloVacinalDoses.AnyAsync(d => d.VacinaId == id, cancellationToken))
            throw new InvalidOperationException("Esta vacina possui histórico ou integra um protocolo. Inative o cadastro para preservá-lo.");

        context.Vacinas.Remove(vacina);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<int> Count(VacinaSearchDto? search = null, CancellationToken cancellationToken = default)
    {
        var query = context.Vacinas.AsNoTracking();
        query = ApplyFilters(query, search);
        return query.CountAsync(cancellationToken);
    }

    public Task<List<Vacina>> GetAllByNameAsync(int page, string name, VacinaSearchDto? search = null, CancellationToken cancellationToken = default)
    {
        var query = context.Vacinas
             .Include(e => e.Especie)
             .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(e => EF.Functions.ILike(e.Name, $"%{name}%"));
        }

        query = ApplyFilters(query, search);

        var currentPage = Pagination.NormalizePage(page);
        return query
             .OrderBy(e => e.Name)
             .Skip(Pagination.Offset(currentPage, 10))
             .Take(10)
             .ToListAsync(cancellationToken);
    }

    private static IQueryable<Vacina> ApplyFilters(IQueryable<Vacina> query, VacinaSearchDto? search)
    {
        if (search is null)
            return query;

        if (search.Ativo.HasValue)
            query = query.Where(v => v.Ativo == search.Ativo.Value);

        if (search.EspecieId.HasValue)
            query = query.Where(vacina => vacina.EspecieId == search.EspecieId.Value);

        if (search.ObrigatorioOrgaoSanitario.HasValue)
            query = query.Where(vacina => vacina.ObrigatorioOrgaoSanitario == search.ObrigatorioOrgaoSanitario.Value);

        return query;
    }
}
