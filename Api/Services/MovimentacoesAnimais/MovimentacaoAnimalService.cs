using Api.Database;
using Api.DTOs;
using Api.Models;
using Api.Models.Enums;
using Api.Services;
using Api.Services.Ambientes;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.MovimentacoesAnimais;

public sealed class MovimentacaoAnimalService(ApiContext context, ClinicalCalendar calendar, AmbienteService ambientes) : IMovimentacaoAnimalService
{
    private const int PageSize = 10;

    public Task<List<MovimentacaoAnimal>> GetAllAsync(int? page, MovimentacaoAnimalSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(BuildQuery(), search);
        query = query.OrderByDescending(m => m.DataMovimentacao)
            .ThenByDescending(m => m.CreationDateTime).ThenByDescending(m => m.Id);

        if (page.HasValue)
            query = query.Skip(Pagination.Offset(page, PageSize)).Take(PageSize);

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
        var movimentacoes = await CreateManyAsync(new MovimentacaoAnimalLoteCreateDto
        {
            AnimalIds = [animalId],
            DataMovimentacao = dto.DataMovimentacao,
            Tipo = dto.Tipo,
            LocalOrigem = dto.LocalOrigem,
            LocalDestino = dto.LocalDestino,
            AmbienteOrigemId = dto.AmbienteOrigemId,
            AmbienteDestinoId = dto.AmbienteDestinoId,
            Motivo = dto.Motivo,
            Observacoes = dto.Observacoes
        }, cancellationToken);

        return movimentacoes[0];
    }

    public async Task<List<MovimentacaoAnimal>> CreateManyAsync(MovimentacaoAnimalLoteCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var animalIds = dto.AnimalIds.Distinct().ToArray();
        if (animalIds.Length == 0 || animalIds.Any(id => id == Guid.Empty))
            throw new ArgumentException("Selecione pelo menos um animal para a movimentação.");
        if (animalIds.Length != dto.AnimalIds.Count)
            throw new ArgumentException("Um animal não pode aparecer duas vezes na movimentação.");
        if (dto.DataMovimentacao == default || dto.DataMovimentacao > calendar.Today)
            throw new ArgumentException("A movimentação deve ter uma data válida, não futura.");
        if (dto.Tipo == TipoMovimentacaoAnimal.Ajuste && (Normalize(dto.Motivo)?.Length ?? 0) < 3)
            throw new ArgumentException("Ajustes exigem um motivo de pelo menos 3 caracteres.");

        var origem = await ambientes.ResolveAsync(dto.AmbienteOrigemId, dto.LocalOrigem, cancellationToken);
        var destino = await ambientes.ResolveAsync(dto.AmbienteDestinoId, dto.LocalDestino, cancellationToken);
        if (dto.Tipo is TipoMovimentacaoAnimal.Entrada or TipoMovimentacaoAnimal.Transferencia && destino is null)
            throw new ArgumentException("Movimentações de entrada e transferência exigem um ambiente de destino.");
        if (dto.Tipo == TipoMovimentacaoAnimal.Saida && destino is not null)
            throw new ArgumentException("Movimentações de saída não têm ambiente de destino.");

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var animals = await context.Animals
            .FromSqlInterpolated($"SELECT *, xmin FROM \"Animals\" WHERE \"Id\" = ANY({animalIds}) ORDER BY \"Id\" FOR UPDATE")
            .ToListAsync(cancellationToken);
        var foundIds = animals.Select(animal => animal.Id).ToHashSet();
        var missingId = animalIds.FirstOrDefault(id => !foundIds.Contains(id));
        if (missingId != Guid.Empty)
            throw new KeyNotFoundException($"Animal com Id '{missingId}' não encontrado.");
        if (animals.Any(a => !a.Ativo) && dto.Tipo != TipoMovimentacaoAnimal.Ajuste)
            throw new ArgumentException("Movimentações de animais inativos devem ser registradas como ajuste justificado.");
        if (animals.Any(a => dto.DataMovimentacao < a.DataNascimento))
            throw new ArgumentException("A movimentação não pode ser anterior ao nascimento do animal.");

        var grupoId = Guid.NewGuid();
        var motivo = Normalize(dto.Motivo);
        var observacoes = Normalize(dto.Observacoes);
        var positions = await context.Animals.Where(a => animalIds.Contains(a.Id)).Select(a => new
        {
            a.Id, HasHistory = a.Movimentacoes.Any(),
            Prior = a.Movimentacoes.Where(m => m.DataMovimentacao <= dto.DataMovimentacao)
                .OrderByDescending(m => m.DataMovimentacao).ThenByDescending(m => m.CreationDateTime).ThenByDescending(m => m.Id)
                .Select(m => new { m.Tipo, m.LocalDestino, m.AmbienteDestinoId }).FirstOrDefault()
        }).ToDictionaryAsync(x => x.Id, cancellationToken);
        var movimentacoes = new List<MovimentacaoAnimal>(animals.Count);
        foreach (var animal in animals)
        {
            var position = positions[animal.Id];
            var (expectedId, expectedName) = position.Prior switch
            {
                { Tipo: TipoMovimentacaoAnimal.Saida } => (null, null),
                { } prior => (prior.AmbienteDestinoId, prior.LocalDestino),
                null when position.HasHistory => (null, null),
                _ => (animal.AmbienteId, animal.LoteOuPasto)
            };
            if (dto.Tipo is TipoMovimentacaoAnimal.Transferencia or TipoMovimentacaoAnimal.Saida &&
                origem is not null && (expectedId.HasValue || expectedName is not null) &&
                !MatchesEnvironment(origem, expectedId, expectedName))
                throw new ArgumentException("O ambiente de origem não confere com o histórico na data informada.");
            if (dto.Tipo == TipoMovimentacaoAnimal.Transferencia && destino is not null &&
                MatchesEnvironment(destino, expectedId ?? origem?.Id, expectedName ?? origem?.Nome))
                throw new ArgumentException("Pelo menos um animal já está no ambiente de destino. Revise a seleção.");
            movimentacoes.Add(new MovimentacaoAnimal
            {
                Id = Guid.NewGuid(),
                GrupoId = grupoId,
                AnimalId = animal.Id,
                DataMovimentacao = dto.DataMovimentacao,
                Tipo = dto.Tipo,
                LocalOrigem = origem?.Nome ?? expectedName,
                AmbienteOrigemId = origem?.Id ?? expectedId,
                LocalDestino = destino?.Nome,
                AmbienteDestinoId = destino?.Id,
                Motivo = motivo,
                Observacoes = observacoes
            });
        }

        // O histórico é append-only: uma correção deve gerar um novo ajuste.
        // O GrupoId permite reconstruir uma operação que envolveu vários animais.
        context.MovimentacoesAnimais.AddRange(movimentacoes);
        await context.SaveChangesAsync(cancellationToken);
        var latest = await context.Animals.Where(a => animalIds.Contains(a.Id)).Select(a => new
        {
            a.Id,
            Last = a.Movimentacoes.OrderByDescending(m => m.DataMovimentacao)
                .ThenByDescending(m => m.CreationDateTime).ThenByDescending(m => m.Id)
                .Select(m => new { m.Tipo, m.LocalDestino, m.AmbienteDestinoId }).FirstOrDefault()
        }).ToDictionaryAsync(x => x.Id, cancellationToken);
        foreach (var animal in animals)
        {
            var last = latest[animal.Id].Last!;
            animal.LoteOuPasto = last.Tipo == TipoMovimentacaoAnimal.Saida ? null : last.LocalDestino;
            animal.AmbienteId = last.Tipo == TipoMovimentacaoAnimal.Saida ? null : last.AmbienteDestinoId;
        }
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await BuildQuery()
            .Where(movimentacao => movimentacao.GrupoId == grupoId)
            .OrderBy(movimentacao => movimentacao.AnimalId)
            .ToListAsync(cancellationToken);
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
        if (search.AmbienteOrigemId.HasValue)
            query = query.Where(m => m.AmbienteOrigemId == search.AmbienteOrigemId);
        if (search.AmbienteDestinoId.HasValue)
            query = query.Where(m => m.AmbienteDestinoId == search.AmbienteDestinoId);
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

    private static bool MatchesEnvironment(Ambiente environment, Guid? expectedId, string? expectedName) =>
        expectedId.HasValue ? environment.Id == expectedId.Value
            : string.Equals(environment.Nome, Normalize(expectedName), StringComparison.OrdinalIgnoreCase);
}
