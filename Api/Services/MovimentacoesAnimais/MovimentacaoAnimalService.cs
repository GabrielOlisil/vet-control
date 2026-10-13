using Api.Database;
using Api.DTOs;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.MovimentacoesAnimais;

public sealed class MovimentacaoAnimalService(ApiContext context) : IMovimentacaoAnimalService
{
    private const int PageSize = 10;

    public Task<List<MovimentacaoAnimal>> GetAllAsync(int? page, MovimentacaoAnimalSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(BuildQuery(), search);
        query = query.OrderByDescending(m => m.DataMovimentacao)
            .ThenByDescending(m => m.CreationDateTime);

        if (page.HasValue)
            query = query.Skip((page.Value - 1) * PageSize).Take(PageSize);

        return query.ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(MovimentacaoAnimalSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        return ApplyFilters(context.MovimentacoesAnimais.AsNoTracking(), search)
            .CountAsync(cancellationToken);
    }

    public Task<MovimentacaoAnimal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return BuildQuery().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<MovimentacaoAnimal> CreateAsync(Guid animalId, MovimentacaoAnimalCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var animal = await context.Animals.FindAsync([animalId], cancellationToken);
        if (animal is null)
            throw new KeyNotFoundException($"Animal com Id '{animalId}' não encontrado.");

        var movimentacao = new MovimentacaoAnimal
        {
            Id = Guid.NewGuid(),
            AnimalId = animalId,
            DataMovimentacao = dto.DataMovimentacao,
            Tipo = dto.Tipo,
            LocalOrigem = Normalize(dto.LocalOrigem),
            LocalDestino = Normalize(dto.LocalDestino),
            Motivo = Normalize(dto.Motivo),
            Observacoes = Normalize(dto.Observacoes)
        };

        context.MovimentacoesAnimais.Add(movimentacao);
        await context.SaveChangesAsync(cancellationToken);
        return (await GetByIdAsync(movimentacao.Id, cancellationToken))!;
    }

    private IQueryable<MovimentacaoAnimal> BuildQuery()
    {
        return context.MovimentacoesAnimais
            .AsNoTracking()
            .Include(m => m.Animal)
                .ThenInclude(a => a!.Identificadores);
    }

    private static IQueryable<MovimentacaoAnimal> ApplyFilters(IQueryable<MovimentacaoAnimal> query,
        MovimentacaoAnimalSearchDto? search)
    {
        if (search is null)
            return query;

        if (search.AnimalId.HasValue)
            query = query.Where(m => m.AnimalId == search.AnimalId.Value);
        if (search.Tipo.HasValue)
            query = query.Where(m => m.Tipo == search.Tipo.Value);
        if (search.DataMovimentacaoFrom.HasValue)
            query = query.Where(m => m.DataMovimentacao >= search.DataMovimentacaoFrom.Value);
        if (search.DataMovimentacaoTo.HasValue)
            query = query.Where(m => m.DataMovimentacao <= search.DataMovimentacaoTo.Value);
        if (!string.IsNullOrWhiteSpace(search.LocalOrigem))
            query = query.Where(m => m.LocalOrigem != null && EF.Functions.ILike(m.LocalOrigem, $"%{search.LocalOrigem}%"));
        if (!string.IsNullOrWhiteSpace(search.LocalDestino))
            query = query.Where(m => m.LocalDestino != null && EF.Functions.ILike(m.LocalDestino, $"%{search.LocalDestino}%"));

        return query;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
