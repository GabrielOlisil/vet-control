using Api.Database;
using Api.DTOs;
using Api.Mappers;
using Api.Models;
using Api.Models.Enums;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Api.Services.AgendaVacinal;

namespace Api.Services.ProtocolosVacinais;

public sealed class ProtocoloVacinalService(ApiContext context, AgendaVacinalService agenda, ClinicalCalendar calendar) : IProtocoloVacinalService
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
            query = query.Skip(Pagination.Offset(page, PageSize)).Take(PageSize);
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
        ValidateScheduleRange(dto.Doses);
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
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var protocolo = await LockProtocol(id)
            .Include(p => p.Doses)
            .FirstOrDefaultAsync(cancellationToken);

        if (protocolo is null)
            return null;

        if (!protocolo.VersaoAtual)
            throw new InvalidOperationException("Uma versão histórica não pode ser editada. Edite a versão atual do protocolo.");

        var changesDefinition = dto.Nome is not null || dto.DescricaoWasProvided || dto.FinalidadeWasProvided ||
            dto.EspecieIdWasProvided || dto.IdadeMinimaDiasWasProvided || dto.IdadeMaximaDiasWasProvided || dto.DosesWasProvided;
        var used = await context.AnimalProtocolosVacinais.AnyAsync(a => a.ProtocoloVacinalId == id, cancellationToken);
        if (used && changesDefinition)
        {
            var prior = protocolo;
            prior.VersaoAtual = false;
            await context.SaveChangesAsync(cancellationToken);
            protocolo = new ProtocoloVacinal
            {
                Id = Guid.NewGuid(), FamiliaId = prior.FamiliaId, Versao = prior.Versao + 1,
                Nome = prior.Nome, Descricao = prior.Descricao, Finalidade = prior.Finalidade,
                EspecieId = prior.EspecieId, Ativo = prior.Ativo,
                IdadeMinimaDias = prior.IdadeMinimaDias, IdadeMaximaDias = prior.IdadeMaximaDias,
                Doses = prior.Doses.Select(d => CreateDose(ToDoseDefinition(d))).ToList()
            };
            context.ProtocolosVacinais.Add(protocolo);
        }

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

        if (dto.IdadeMinimaDiasWasProvided)
            protocolo.IdadeMinimaDias = dto.IdadeMinimaDias;

        if (dto.IdadeMaximaDiasWasProvided)
            protocolo.IdadeMaximaDias = dto.IdadeMaximaDias;

        if (dto.DosesWasProvided)
        {
            if (dto.Doses is null || dto.Doses.Count == 0)
                throw new ArgumentException("O protocolo deve conter pelo menos uma dose.");

            ValidateProtocol(protocolo.IdadeMinimaDias, protocolo.IdadeMaximaDias, dto.Doses);
            ValidateScheduleRange(dto.Doses);
            await ValidateReferencesAsync(protocolo.EspecieId, dto.Doses, cancellationToken);

            context.ProtocoloVacinalDoses.RemoveRange(protocolo.Doses.ToArray());
            protocolo.Doses = dto.Doses.Select(CreateDose).ToList();
            context.ProtocoloVacinalDoses.AddRange(protocolo.Doses);
        }
        else
        {
            ValidateProtocol(protocolo.IdadeMinimaDias, protocolo.IdadeMaximaDias, protocolo.Doses
                .Select(ToDoseDefinition).ToList());
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
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

        if (await context.ProtocolosVacinais.CountAsync(p => p.FamiliaId == protocolo.FamiliaId, cancellationToken) > 1)
            throw new InvalidOperationException("Não é possível excluir uma versão de um protocolo com histórico de versões.");

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
        if (dto.DataInicio == default || dto.DataInicio > calendar.Today)
            throw new ArgumentException("O início do protocolo deve ser uma data válida, não futura.");
        await using var transaction = context.Database.CurrentTransaction is null
            ? await context.Database.BeginTransactionAsync(cancellationToken) : null;
        var animal = await context.Animals
            .FromSqlInterpolated($"SELECT *, xmin FROM \"Animals\" WHERE \"Id\" = {animalId} FOR UPDATE")
            .Include(a => a.Raca)
            .FirstOrDefaultAsync(a => a.Id == animalId, cancellationToken)
            ?? throw new KeyNotFoundException($"Animal com Id '{animalId}' não encontrado.");

        var protocolo = await LockProtocol(dto.ProtocoloVacinalId)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Protocolo com Id '{dto.ProtocoloVacinalId}' não encontrado.");

        if (!protocolo.Ativo)
            throw new ArgumentException("O protocolo selecionado está inativo.");
        if (!protocolo.VersaoAtual)
            throw new ArgumentException("Novas atribuições devem usar a versão atual do protocolo.");
        if (!animal.Ativo)
            throw new ArgumentException("Não é possível atribuir protocolo a um animal inativo.");

        if (protocolo.EspecieId.HasValue && animal.Raca?.EspecieId != protocolo.EspecieId)
            throw new ArgumentException("O protocolo não é compatível com a espécie do animal.");

        var idadeDias = dto.DataInicio.DayNumber - animal.DataNascimento.DayNumber;
        if (idadeDias < 0) throw new ArgumentException("O início do protocolo não pode ser anterior ao nascimento.");
        if (protocolo.IdadeMinimaDias.HasValue && idadeDias < protocolo.IdadeMinimaDias)
            throw new ArgumentException("O animal ainda não atingiu a idade mínima do protocolo.");
        if (protocolo.IdadeMaximaDias.HasValue && idadeDias > protocolo.IdadeMaximaDias)
            throw new ArgumentException("O animal ultrapassou a idade máxima do protocolo.");

        var alreadyAssigned = await context.AnimalProtocolosVacinais.AnyAsync(p =>
            p.AnimalId == animalId &&
            p.ProtocoloVacinal!.FamiliaId == protocolo.FamiliaId &&
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
        if (transaction is not null) await transaction.CommitAsync(cancellationToken);
        return (await GetAnimalProtocolByIdAsync(assignment.Id, cancellationToken))!;
    }

    public async Task<AnimalProtocoloVacinal?> PatchAnimalProtocolAsync(Guid animalProtocolId,
        AnimalProtocoloVacinalPatchDto dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var assignment = await context.AnimalProtocolosVacinais
            .Include(p => p.ProtocoloVacinal)
            .FirstOrDefaultAsync(p => p.Id == animalProtocolId, cancellationToken);

        if (assignment is null)
            return null;

        var animal = await context.Animals.FromSqlInterpolated($"SELECT *, xmin FROM \"Animals\" WHERE \"Id\" = {assignment.AnimalId} FOR UPDATE")
            .Include(a => a.Raca).SingleAsync(cancellationToken);
        if (dto.DataInicio.HasValue || (dto.Ativo == true && !assignment.Ativo))
        {
            var start = dto.DataInicio ?? assignment.DataInicio;
            var protocol = assignment.ProtocoloVacinal!;
            var age = start.DayNumber - animal.DataNascimento.DayNumber;
            if (start == default || start > calendar.Today || age < 0 ||
                (protocol.IdadeMinimaDias.HasValue && age < protocol.IdadeMinimaDias.Value) ||
                (protocol.IdadeMaximaDias.HasValue && age > protocol.IdadeMaximaDias.Value))
                throw new ArgumentException("A data de início não atende aos limites de idade ou à data da clínica.");
            if (!animal.Ativo || !protocol.Ativo || (protocol.EspecieId.HasValue && protocol.EspecieId != animal.Raca?.EspecieId))
                throw new ArgumentException("O animal ou protocolo está inativo ou possui espécie incompatível.");
            if (dto.Ativo == true && !assignment.Ativo && !protocol.VersaoAtual)
                throw new InvalidOperationException("Use a migração explícita para a versão atual em vez de reativar uma versão histórica.");
        }
        if (dto.DataInicio.HasValue && dto.DataInicio != assignment.DataInicio &&
            await context.AplicacoesVacina.AnyAsync(a => a.AnimalProtocoloVacinalId == animalProtocolId, cancellationToken))
            throw new InvalidOperationException("Não altere o início de um esquema já utilizado. Encerre-o e atribua um novo esquema.");
        if (dto.Ativo == true && await context.AnimalProtocolosVacinais.AnyAsync(p => p.Id != animalProtocolId &&
            p.AnimalId == assignment.AnimalId && p.Ativo &&
            p.ProtocoloVacinal!.FamiliaId == assignment.ProtocoloVacinal!.FamiliaId, cancellationToken))
            throw new InvalidOperationException("Já existe uma versão desta família de protocolo ativa para o animal.");

        if (dto.DataInicio.HasValue)
            assignment.DataInicio = dto.DataInicio.Value;
        if (dto.Ativo.HasValue)
            assignment.Ativo = dto.Ativo.Value;
        if (dto.ObservacoesWasProvided)
            assignment.Observacoes = Normalize(dto.Observacoes);

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetAnimalProtocolByIdAsync(animalProtocolId, cancellationToken);
    }

    public async Task<AnimalProtocoloVacinal?> MigrarAsync(Guid animalId, Guid vinculoId,
        MigracaoProtocoloVacinalDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Motivo) || dto.Motivo.Trim().Length < 3)
            throw new ArgumentException("Informe o motivo da migração.");
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await context.Animals.FromSqlInterpolated($"SELECT *, xmin FROM \"Animals\" WHERE \"Id\" = {animalId} FOR UPDATE")
            .LoadAsync(cancellationToken);
        var original = await context.AnimalProtocolosVacinais.Include(p => p.ProtocoloVacinal)
            .SingleOrDefaultAsync(p => p.Id == vinculoId && p.AnimalId == animalId, cancellationToken);
        if (original is null) return null;
        var target = await context.ProtocolosVacinais.SingleOrDefaultAsync(p => p.Id == dto.ProtocoloDestinoId, cancellationToken);
        if (!original.Ativo || target is null || target.FamiliaId != original.ProtocoloVacinal!.FamiliaId ||
            target.Versao <= original.ProtocoloVacinal.Versao || !target.VersaoAtual)
            throw new ArgumentException("A migração exige uma atribuição ativa e uma versão atual mais recente da mesma família.");
        original.Ativo = false;
        original.Observacoes = $"{original.Observacoes}\nMigrado para versão {target.Versao}: {dto.Motivo.Trim()}".Trim();
        await context.SaveChangesAsync(cancellationToken);
        var replacement = await AssignToAnimalAsync(animalId, new AnimalProtocoloVacinalCreateDto
        {
            ProtocoloVacinalId = target.Id, DataInicio = dto.DataInicio,
            Observacoes = $"Migração do vínculo {vinculoId}. {dto.Motivo.Trim()}. As doses históricas permanecem no vínculo original; o novo esquema começa nesta data."
        }, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return replacement;
    }

    public async Task<List<ProtocoloVacinalPendenciaResponseDto>> GetPendenciasAsync(int? page,
        ProtocoloVacinalPendenciaSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var items = await agenda.GetAsync(page, AgendaSearch(search), cancellationToken);
        return items.Select(p => new ProtocoloVacinalPendenciaResponseDto
        {
            AnimalProtocoloVacinalId = p.AnimalProtocoloVacinalId!.Value, AnimalId = p.AnimalId,
            ProtocoloVacinalId = p.ProtocoloVacinalId!.Value, ProtocoloVacinalDoseId = p.ProtocoloVacinalDoseId!.Value,
            Animal = new AnimalShortResponseDto { Id = p.AnimalId, Name = p.AnimalNome, IdentificadorPrincipal = p.IdentificadorPrincipal },
            ProtocoloVacinal = new ProtocoloVacinalShortResponseDto { Id = p.ProtocoloVacinalId.Value, Nome = p.ProtocoloNome ?? "", Versao = p.ProtocoloVersao ?? 1 },
            Vacina = new VacinaShortResponseDto { Id = p.VacinaId, Name = p.VacinaNome },
            Ordem = p.Ordem ?? 1, DataPrevista = p.DataPrevista, DataAplicacao = p.DataAplicacao,
            AplicacaoVacinaId = p.AplicacaoVacinaId, Obrigatoria = p.Obrigatoria, Status = p.Status
        }).ToList();
    }

    public async Task<int> CountPendenciasAsync(ProtocoloVacinalPendenciaSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        return await agenda.CountAsync(AgendaSearch(search), cancellationToken);
    }

    private static AgendaVacinalSearchDto AgendaSearch(ProtocoloVacinalPendenciaSearchDto? search) => new()
    {
        Origem = OrigemPendenciaVacinal.Protocolo, AnimalId = search?.AnimalId,
        ProtocoloVacinalId = search?.ProtocoloVacinalId, EspecieId = search?.EspecieId,
        DataPrevistaFrom = search?.DataPrevistaFrom, DataPrevistaTo = search?.DataPrevistaTo,
        Status = search?.Status, SomenteObrigatorias = search?.SomenteObrigatorias == true
    };

    // Editing and assigning must read the same definition after acquiring its lock.
    private IQueryable<ProtocoloVacinal> LockProtocol(Guid id) => context.ProtocolosVacinais
        .FromSqlInterpolated($"SELECT *, xmin FROM \"ProtocolosVacinais\" WHERE \"Id\" = {id} FOR UPDATE");

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


    private static IQueryable<ProtocoloVacinal> ApplyFilters(IQueryable<ProtocoloVacinal> query,
        ProtocoloVacinalSearchDto? search)
    {
        if (search?.IncluirVersoesAnteriores != true)
            query = query.Where(p => p.VersaoAtual);
        if (search is null)
            return query;

        if (search.FamiliaId.HasValue)
            query = query.Where(p => p.FamiliaId == search.FamiliaId.Value);

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

    private void ValidateScheduleRange(IEnumerable<ProtocoloVacinalDoseCreateDto> doses)
    {
        var maxDelay = DateOnly.MaxValue.DayNumber - calendar.Today.DayNumber;
        if (doses.Any(d => d.DiasAposInicio > maxDelay || d.IntervaloReforcoDias > maxDelay))
            throw new ArgumentException("O prazo inicial ou o intervalo de reforço ultrapassa o limite do calendário.");
    }

    private static ProtocoloVacinalDoseCreateDto ToDoseDefinition(ProtocoloVacinalDose dose) => new()
    {
        VacinaId = dose.VacinaId,
        Ordem = dose.Ordem,
        DiasAposInicio = dose.DiasAposInicio,
        IntervaloReforcoDias = dose.IntervaloReforcoDias,
        Obrigatoria = dose.Obrigatoria,
        Observacoes = dose.Observacoes
    };

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
