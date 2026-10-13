using Api.Database;
using Api.DTOs;
using Api.Mappers;
using Api.Models;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.ProtocolosVacinais;

public sealed class ProtocoloVacinalService(ApiContext context) : IProtocoloVacinalService
{
    private const int PageSize = 10;

    public Task<List<ProtocoloVacinal>> GetAllAsync(int? page, ProtocoloVacinalSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.ProtocolosVacinais
            .Include(p => p.Especie)
            .Include(p => p.Doses)
            .Include(p => p.AplicacoesEmAnimais)
            .AsNoTracking();

        query = ApplyFilters(query, search);
        query = query.OrderBy(p => p.Nome);

        if (page.HasValue)
        {
            query = query.Skip((page.Value - 1) * PageSize).Take(PageSize);
        }

        return query.ToListAsync(cancellationToken);
    }

    public Task<ProtocoloVacinal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.ProtocolosVacinais
            .AsNoTracking()
            .Include(p => p.Especie)
            .Include(p => p.Doses)
                .ThenInclude(d => d.Vacina)
            .Include(p => p.AplicacoesEmAnimais)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<ProtocoloVacinal> CreateAsync(ProtocoloVacinalCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        ValidateProtocol(dto.IdadeMinimaDias, dto.IdadeMaximaDias, dto.Doses);
        await ValidateReferencesAsync(dto.EspecieId, dto.Doses, cancellationToken);

        var protocolo = new ProtocoloVacinal
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome.Trim(),
            Descricao = Normalize(dto.Descricao),
            Finalidade = Normalize(dto.Finalidade),
            EspecieId = dto.EspecieId,
            Ativo = dto.Ativo,
            IdadeMinimaDias = dto.IdadeMinimaDias,
            IdadeMaximaDias = dto.IdadeMaximaDias,
            Doses = dto.Doses.Select(CreateDose).ToList()
        };

        context.ProtocolosVacinais.Add(protocolo);
        await context.SaveChangesAsync(cancellationToken);
        return protocolo;
    }

    public async Task<ProtocoloVacinal?> PatchAsync(Guid id, ProtocoloVacinalPatchDto dto,
        CancellationToken cancellationToken = default)
    {
        var protocolo = await context.ProtocolosVacinais
            .Include(p => p.Doses)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (protocolo is null)
            return null;

        if (dto.Nome is not null)
            protocolo.Nome = dto.Nome.Trim();

        if (dto.DescricaoWasProvided)
            protocolo.Descricao = Normalize(dto.Descricao);

        if (dto.FinalidadeWasProvided)
            protocolo.Finalidade = Normalize(dto.Finalidade);

        if (dto.EspecieIdWasProvided)
            protocolo.EspecieId = dto.EspecieId;

        if (dto.Ativo.HasValue)
            protocolo.Ativo = dto.Ativo.Value;

        if (dto.IdadeMinimaDias.HasValue)
            protocolo.IdadeMinimaDias = dto.IdadeMinimaDias.Value;

        if (dto.IdadeMaximaDias.HasValue)
            protocolo.IdadeMaximaDias = dto.IdadeMaximaDias.Value;

        if (dto.DosesWasProvided)
        {
            if (dto.Doses is null || dto.Doses.Count == 0)
                throw new ArgumentException("O protocolo deve conter pelo menos uma dose.");

            ValidateProtocol(protocolo.IdadeMinimaDias, protocolo.IdadeMaximaDias, dto.Doses);
            await ValidateReferencesAsync(protocolo.EspecieId, dto.Doses, cancellationToken);

            context.ProtocoloVacinalDoses.RemoveRange(protocolo.Doses);
            protocolo.Doses = dto.Doses.Select(CreateDose).ToList();
        }
        else
        {
            ValidateProtocol(protocolo.IdadeMinimaDias, protocolo.IdadeMaximaDias, protocolo.Doses
                .Select(d => new ProtocoloVacinalDoseCreateDto
                {
                    VacinaId = d.VacinaId,
                    Ordem = d.Ordem,
                    DiasAposInicio = d.DiasAposInicio,
                    IntervaloReforcoDias = d.IntervaloReforcoDias,
                    Obrigatoria = d.Obrigatoria,
                    Observacoes = d.Observacoes
                }).ToList());
        }

        await context.SaveChangesAsync(cancellationToken);
        return protocolo;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var protocolo = await context.ProtocolosVacinais
            .Include(p => p.AplicacoesEmAnimais)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (protocolo is null)
            return false;

        if (protocolo.AplicacoesEmAnimais.Count > 0)
            throw new InvalidOperationException("Não é possível excluir um protocolo já atribuído a animais. Inative-o para preservar o histórico.");

        context.ProtocolosVacinais.Remove(protocolo);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<int> CountAsync(ProtocoloVacinalSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(context.ProtocolosVacinais.AsNoTracking(), search);
        return query.CountAsync(cancellationToken);
    }

    public Task<AnimalProtocoloVacinal?> GetAnimalProtocolByIdAsync(Guid animalProtocolId,
        CancellationToken cancellationToken = default)
    {
        return BuildAnimalProtocolQuery()
            .FirstOrDefaultAsync(p => p.Id == animalProtocolId, cancellationToken);
    }

    public Task<List<AnimalProtocoloVacinal>> GetAnimalProtocolsAsync(Guid animalId,
        CancellationToken cancellationToken = default)
    {
        return BuildAnimalProtocolQuery()
            .Where(p => p.AnimalId == animalId)
            .OrderByDescending(p => p.Ativo)
            .ThenByDescending(p => p.DataInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<AnimalProtocoloVacinal> AssignToAnimalAsync(Guid animalId,
        AnimalProtocoloVacinalCreateDto dto, CancellationToken cancellationToken = default)
    {
        var animal = await context.Animals
            .Include(a => a.Raca)
            .FirstOrDefaultAsync(a => a.Id == animalId, cancellationToken)
            ?? throw new KeyNotFoundException($"Animal com Id '{animalId}' não encontrado.");

        var protocolo = await context.ProtocolosVacinais
            .Include(p => p.Especie)
            .FirstOrDefaultAsync(p => p.Id == dto.ProtocoloVacinalId, cancellationToken)
            ?? throw new KeyNotFoundException($"Protocolo com Id '{dto.ProtocoloVacinalId}' não encontrado.");

        if (!protocolo.Ativo)
            throw new ArgumentException("O protocolo selecionado está inativo.");

        if (protocolo.EspecieId.HasValue && animal.Raca?.EspecieId != protocolo.EspecieId)
            throw new ArgumentException("O protocolo não é compatível com a espécie do animal.");

        var idadeDias = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - animal.DataNascimento.DayNumber;
        if (protocolo.IdadeMinimaDias.HasValue && idadeDias < protocolo.IdadeMinimaDias)
            throw new ArgumentException("O animal ainda não atingiu a idade mínima do protocolo.");
        if (protocolo.IdadeMaximaDias.HasValue && idadeDias > protocolo.IdadeMaximaDias)
            throw new ArgumentException("O animal ultrapassou a idade máxima do protocolo.");

        var alreadyAssigned = await context.AnimalProtocolosVacinais.AnyAsync(p =>
            p.AnimalId == animalId &&
            p.ProtocoloVacinalId == dto.ProtocoloVacinalId &&
            p.Ativo, cancellationToken);

        if (alreadyAssigned)
            throw new ArgumentException("Este animal já possui este protocolo ativo.");

        var assignment = new AnimalProtocoloVacinal
        {
            Id = Guid.NewGuid(),
            AnimalId = animalId,
            ProtocoloVacinalId = dto.ProtocoloVacinalId,
            DataInicio = dto.DataInicio,
            Ativo = dto.Ativo,
            Observacoes = Normalize(dto.Observacoes)
        };

        context.AnimalProtocolosVacinais.Add(assignment);
        await context.SaveChangesAsync(cancellationToken);
        return (await GetAnimalProtocolByIdAsync(assignment.Id, cancellationToken))!;
    }

    public async Task<AnimalProtocoloVacinal?> PatchAnimalProtocolAsync(Guid animalProtocolId,
        AnimalProtocoloVacinalPatchDto dto, CancellationToken cancellationToken = default)
    {
        var assignment = await context.AnimalProtocolosVacinais
            .FirstOrDefaultAsync(p => p.Id == animalProtocolId, cancellationToken);

        if (assignment is null)
            return null;

        if (dto.DataInicio.HasValue)
            assignment.DataInicio = dto.DataInicio.Value;
        if (dto.Ativo.HasValue)
            assignment.Ativo = dto.Ativo.Value;
        if (dto.ObservacoesWasProvided)
            assignment.Observacoes = Normalize(dto.Observacoes);

        await context.SaveChangesAsync(cancellationToken);
        return await GetAnimalProtocolByIdAsync(animalProtocolId, cancellationToken);
    }

    public async Task<List<ProtocoloVacinalPendenciaResponseDto>> GetPendenciasAsync(int? page,
        ProtocoloVacinalPendenciaSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var items = await BuildPendingQuery(search).ToListAsync(cancellationToken);
        var pending = ApplyPendingFilters(items.SelectMany(BuildPendingItems), search);

        var currentPage = page ?? 1;
        return pending
            .OrderBy(p => p.DataPrevista)
            .ThenBy(p => p.AnimalId)
            .Skip((currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }

    public async Task<int> CountPendenciasAsync(ProtocoloVacinalPendenciaSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var items = await BuildPendingQuery(search).ToListAsync(cancellationToken);
        return ApplyPendingFilters(items.SelectMany(BuildPendingItems), search).Count();
    }

    private IQueryable<AnimalProtocoloVacinal> BuildAnimalProtocolQuery()
    {
        return context.AnimalProtocolosVacinais
            .AsNoTracking()
            .Include(p => p.Animal)
                .ThenInclude(a => a!.Identificadores)
            .Include(p => p.ProtocoloVacinal)
                .ThenInclude(p => p!.Especie)
            .Include(p => p.ProtocoloVacinal)
                .ThenInclude(p => p!.Doses)
                    .ThenInclude(d => d.Vacina)
            .Include(p => p.Aplicacoes)
                .ThenInclude(a => a.ProtocoloVacinalDose);
    }

    private IQueryable<AnimalProtocoloVacinal> BuildPendingQuery(ProtocoloVacinalPendenciaSearchDto? search)
    {
        var query = BuildAnimalProtocolQuery().Where(p => p.Ativo && p.ProtocoloVacinal!.Ativo);

        if (search is null)
            return query;

        if (search.AnimalId.HasValue)
            query = query.Where(p => p.AnimalId == search.AnimalId.Value);
        if (search.ProtocoloVacinalId.HasValue)
            query = query.Where(p => p.ProtocoloVacinalId == search.ProtocoloVacinalId.Value);
        if (search.EspecieId.HasValue)
            query = query.Where(p => p.ProtocoloVacinal!.EspecieId == search.EspecieId.Value);

        return query;
    }

    private static IEnumerable<ProtocoloVacinalPendenciaResponseDto> ApplyPendingFilters(
        IEnumerable<ProtocoloVacinalPendenciaResponseDto> items,
        ProtocoloVacinalPendenciaSearchDto? search)
    {
        if (search is null)
            return items;

        if (search.DataPrevistaFrom.HasValue)
            items = items.Where(p => p.DataPrevista >= search.DataPrevistaFrom.Value);
        if (search.DataPrevistaTo.HasValue)
            items = items.Where(p => p.DataPrevista <= search.DataPrevistaTo.Value);
        if (search.Status.HasValue)
            items = items.Where(p => p.Status == search.Status.Value);
        if (search.SomenteObrigatorias == true)
            items = items.Where(p => p.Obrigatoria);

        return items;
    }

    private IEnumerable<ProtocoloVacinalPendenciaResponseDto> BuildPendingItems(AnimalProtocoloVacinal assignment)
    {
        if (assignment.ProtocoloVacinal is null || assignment.Animal is null)
            return [];

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var items = new List<ProtocoloVacinalPendenciaResponseDto>();

        foreach (var dose in assignment.ProtocoloVacinal.Doses.OrderBy(d => d.Ordem))
        {
            var application = assignment.Aplicacoes
                .Where(a => a.ProtocoloVacinalDoseId == dose.Id)
                .OrderByDescending(a => a.DataAplicacao)
                .FirstOrDefault();

            var dataPrevista = assignment.DataInicio.AddDays(dose.DiasAposInicio);
            var status = StatusPendenciaVacinal.Pendente;

            if (application is not null)
            {
                if (dose.IntervaloReforcoDias.HasValue)
                {
                    dataPrevista = application.DataAplicacao.AddDays(dose.IntervaloReforcoDias.Value);
                }
                else
                {
                    status = StatusPendenciaVacinal.Concluida;
                }
            }

            if (status != StatusPendenciaVacinal.Concluida && dataPrevista < today)
                status = StatusPendenciaVacinal.Atrasada;

            items.Add(new ProtocoloVacinalPendenciaResponseDto
            {
                AnimalProtocoloVacinalId = assignment.Id,
                AnimalId = assignment.AnimalId,
                ProtocoloVacinalId = assignment.ProtocoloVacinalId,
                ProtocoloVacinalDoseId = dose.Id,
                Animal = AnimalMapper.MapToShort(assignment.Animal),
                ProtocoloVacinal = ProtocoloVacinalMapper.MapToShort(assignment.ProtocoloVacinal),
                Vacina = dose.Vacina is null ? null : VacinaMapper.MapToShort(dose.Vacina),
                Ordem = dose.Ordem,
                DataPrevista = dataPrevista,
                DataAplicacao = application?.DataAplicacao,
                AplicacaoVacinaId = application?.Id,
                Obrigatoria = dose.Obrigatoria,
                Status = status
            });
        }

        return items;
    }

    private static IQueryable<ProtocoloVacinal> ApplyFilters(IQueryable<ProtocoloVacinal> query,
        ProtocoloVacinalSearchDto? search)
    {
        if (search is null)
            return query;

        if (search.EspecieId.HasValue)
            query = query.Where(p => p.EspecieId == search.EspecieId.Value);
        if (search.Ativo.HasValue)
            query = query.Where(p => p.Ativo == search.Ativo.Value);
        if (!string.IsNullOrWhiteSpace(search.Nome))
            query = query.Where(p => EF.Functions.ILike(p.Nome, $"%{search.Nome}%"));
        if (!string.IsNullOrWhiteSpace(search.Finalidade))
            query = query.Where(p => p.Finalidade == search.Finalidade);

        return query;
    }

    private async Task ValidateReferencesAsync(Guid? especieId,
        IEnumerable<ProtocoloVacinalDoseCreateDto> doses, CancellationToken cancellationToken)
    {
        if (especieId.HasValue && !await context.Especies.AnyAsync(e => e.Id == especieId.Value, cancellationToken))
            throw new KeyNotFoundException($"Espécie com Id '{especieId}' não encontrada.");

        var ids = doses.Select(d => d.VacinaId).Distinct().ToList();
        var count = await context.Vacinas.CountAsync(v => ids.Contains(v.Id), cancellationToken);
        if (count != ids.Count)
            throw new KeyNotFoundException("Uma ou mais vacinas informadas não foram encontradas.");
    }

    private static void ValidateProtocol(int? idadeMinimaDias, int? idadeMaximaDias,
        IReadOnlyCollection<ProtocoloVacinalDoseCreateDto> doses)
    {
        if (idadeMinimaDias.HasValue && idadeMaximaDias.HasValue && idadeMinimaDias > idadeMaximaDias)
            throw new ArgumentException("A idade mínima não pode ser maior que a idade máxima.");

        if (doses.Count == 0)
            throw new ArgumentException("O protocolo deve conter pelo menos uma dose.");

        var ordens = doses.Select(d => d.Ordem).ToList();
        if (ordens.Any(o => o < 1) || ordens.Distinct().Count() != ordens.Count)
            throw new ArgumentException("As doses devem possuir ordens positivas e únicas.");

        if (doses.Any(d => d.DiasAposInicio < 0 || d.IntervaloReforcoDias is <= 0))
            throw new ArgumentException("Os intervalos do protocolo devem ser válidos.");
    }

    private static ProtocoloVacinalDose CreateDose(ProtocoloVacinalDoseCreateDto dose) => new()
    {
        Id = Guid.NewGuid(),
        VacinaId = dose.VacinaId,
        Ordem = dose.Ordem,
        DiasAposInicio = dose.DiasAposInicio,
        IntervaloReforcoDias = dose.IntervaloReforcoDias,
        Obrigatoria = dose.Obrigatoria,
        Observacoes = Normalize(dose.Observacoes)
    };

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
