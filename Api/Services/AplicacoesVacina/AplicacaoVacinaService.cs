using Api.Database;
using Api.DTOs;
using Api.Mappers;
using Api.Models;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.Services.AplicacoesVacina;

public sealed class AplicacaoVacinaService(ApiContext context, IHttpContextAccessor httpContextAccessor) : IAplicacaoVacinaService
{
    public async Task<AplicacaoVacina> CreateAsync(AplicacaoVacinaCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetAuthenticatedUser();
        var animal = await context.Animals.FindAsync([dto.AnimalId], cancellationToken);
        if (animal is null)
            throw new KeyNotFoundException($"Animal com Id '{dto.AnimalId}' não encontrado.");

        var vacinaExists = await context.Vacinas.AnyAsync(v => v.Id == dto.VacinaId, cancellationToken);
        if (!vacinaExists)
            throw new KeyNotFoundException($"Vacina com Id '{dto.VacinaId}' não encontrada.");

        ValidateVaccineDates(dto.DataFabricacao, dto.DataValidade);

        var protocolLink = await ValidateProtocolLinkAsync(
            dto.AnimalId,
            dto.VacinaId,
            dto.AnimalProtocoloVacinalId,
            dto.ProtocoloVacinalDoseId,
            cancellationToken);

        var dataProximaDose = dto.DataProximaDose;
        if (dataProximaDose is null)
        {
            if (protocolLink is not null)
            {
                dataProximaDose = CalculateNextDoseDate(protocolLink.Value.Assignment,
                    protocolLink.Value.Dose, dto.DataAplicacao);
            }
            else
            {
                var vacina = await context.Vacinas.FindAsync([dto.VacinaId], cancellationToken);
                if (vacina is not null && vacina.ReaplicarEmXDias > 0)
                    dataProximaDose = dto.DataAplicacao.AddDays((int)vacina.ReaplicarEmXDias);
            }
        }

        var aplicacao = new AplicacaoVacina
        {
            Id = Guid.NewGuid(),
            AnimalId = dto.AnimalId,
            VacinaId = dto.VacinaId,
            AnimalProtocoloVacinalId = dto.AnimalProtocoloVacinalId,
            ProtocoloVacinalDoseId = dto.ProtocoloVacinalDoseId,
            DataAplicacao = dto.DataAplicacao,
            DataProximaDose = dataProximaDose,
            CicloFinalizado = dto.CicloFinalizado ?? true,
            StatusComprovante = StatusComprovanteVacina.PendenteAssinatura,
            NumeroLote = dto.NumeroLote,
            DoseMl = dto.DoseMl,
            DataFabricacao = dto.DataFabricacao,
            DataValidade = dto.DataValidade,
            LocalAplicacao = dto.LocalAplicacao,
            Observacoes = dto.Observacoes,
            VeterinarioResponsavel = currentUser.Name,
            VeterinarioCrmv = currentUser.Crmv,
            VeterinarioCrmvUf = currentUser.CrmvUf,
            Aplicador = currentUser.Name,
            LaboratorioFabricante = dto.LaboratorioFabricante
        };

        context.AplicacoesVacina.Add(aplicacao);
        await context.SaveChangesAsync(cancellationToken);

        return aplicacao;
    }

    public async Task<AplicacaoVacina?> PatchAsync(Guid id, AplicacaoVacinaPatchDto dto,
        CancellationToken cancellationToken = default)
    {
        var aplicacao =
            await context.AplicacoesVacina.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (aplicacao is null)
        {
            return null;
        }

        EnsureNotSigned(aplicacao);

        if (dto.AnimalId.HasValue)
        {
            var animalExists = await context.Animals.AnyAsync(a => a.Id == dto.AnimalId.Value, cancellationToken);
            if (!animalExists)
                throw new KeyNotFoundException($"Animal com Id '{dto.AnimalId}' não encontrado.");
            aplicacao.AnimalId = dto.AnimalId.Value;
        }

        if (dto.VacinaId.HasValue)
        {
            var vacinaExists = await context.Vacinas.AnyAsync(v => v.Id == dto.VacinaId.Value, cancellationToken);
            if (!vacinaExists)
                throw new KeyNotFoundException($"Vacina com Id '{dto.VacinaId}' não encontrada.");
            aplicacao.VacinaId = dto.VacinaId.Value;
        }

        if (dto.AnimalProtocoloVacinalIdWasProvided)
        {
            aplicacao.AnimalProtocoloVacinalId = dto.AnimalProtocoloVacinalId;
        }

        if (dto.ProtocoloVacinalDoseIdWasProvided)
        {
            aplicacao.ProtocoloVacinalDoseId = dto.ProtocoloVacinalDoseId;
        }

        var protocolLink = await ValidateProtocolLinkAsync(
            aplicacao.AnimalId,
            aplicacao.VacinaId,
            aplicacao.AnimalProtocoloVacinalId,
            aplicacao.ProtocoloVacinalDoseId,
            cancellationToken);

        if (dto.DataAplicacao.HasValue)
        {
            aplicacao.DataAplicacao = dto.DataAplicacao.Value;
        }

        if (dto.DataProximaDoseWasProvided)
        {
            aplicacao.DataProximaDose = dto.DataProximaDose;
        }

        if (dto.CicloFinalizado.HasValue)
        {
            aplicacao.CicloFinalizado = dto.CicloFinalizado.Value;
        }

        if (dto.NumeroLote is not null)
        {
            if (string.IsNullOrWhiteSpace(dto.NumeroLote))
                throw new ArgumentException("O número do lote não pode ser vazio.");
            aplicacao.NumeroLote = dto.NumeroLote;
        }

        if (dto.DoseMlWasProvided)
        {
            aplicacao.DoseMl = dto.DoseMl;
        }

        if (dto.DataFabricacaoWasProvided)
        {
            aplicacao.DataFabricacao = dto.DataFabricacao;
        }

        if (dto.DataValidadeWasProvided)
        {
            aplicacao.DataValidade = dto.DataValidade;
        }

        ValidateVaccineDates(aplicacao.DataFabricacao, aplicacao.DataValidade);

        if (dto.LocalAplicacaoWasProvided)
        {
            aplicacao.LocalAplicacao = dto.LocalAplicacao;
        }

        if (dto.ObservacoesWasProvided)
        {
            aplicacao.Observacoes = dto.Observacoes;
        }

        if (dto.LaboratorioFabricanteWasProvided)
        {
            aplicacao.LaboratorioFabricante = dto.LaboratorioFabricante;
        }

        if (protocolLink is not null && !dto.DataProximaDoseWasProvided &&
            (dto.AnimalProtocoloVacinalIdWasProvided || dto.ProtocoloVacinalDoseIdWasProvided ||
             dto.DataAplicacao.HasValue))
        {
            aplicacao.DataProximaDose = CalculateNextDoseDate(protocolLink.Value.Assignment,
                protocolLink.Value.Dose, aplicacao.DataAplicacao);
        }

        await context.SaveChangesAsync(cancellationToken);

        return aplicacao;
    }

    public async Task<AplicacaoVacina?> AssinarAsync(Guid id, AplicacaoVacinaAssinaturaDto dto,
        CancellationToken cancellationToken = default)
    {
        var aplicacao = await context.AplicacoesVacina
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (aplicacao is null)
            return null;

        if (aplicacao.StatusComprovante == StatusComprovanteVacina.Assinado)
            throw new InvalidOperationException("Esta aplicação já está assinada e não pode ser assinada novamente.");

        var nome = dto.NomeAssinante.Trim();
        var crmv = dto.Crmv.Trim();
        var uf = NormalizeUf(dto.CrmvUf);
        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(crmv) || string.IsNullOrWhiteSpace(uf))
            throw new ArgumentException("Informe nome, CRMV e UF do veterinário assinante.");

        // A assinatura usa precisão de minutos. Isso torna o timestamp usado no
        // hash idêntico ao timestamp persistido, sem depender de frações de segundo.
        var assinaturaEm = NormalizeSignatureTimestamp(DateTimeOffset.UtcNow);
        var hash = CalculateSignatureHash(aplicacao, nome, crmv, uf, assinaturaEm);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var updatedRows = await context.AplicacoesVacina
                .Where(a => a.Id == id && a.StatusComprovante != StatusComprovanteVacina.Assinado)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.StatusComprovante, StatusComprovanteVacina.Assinado)
                    .SetProperty(a => a.AssinadaEm, assinaturaEm)
                    .SetProperty(a => a.Assinante, nome)
                    .SetProperty(a => a.AssinanteCrmv, crmv)
                    .SetProperty(a => a.AssinanteCrmvUf, uf)
                    .SetProperty(a => a.HashAssinatura, hash)
                    .SetProperty(a => a.MetodoAssinatura, "InternaAuditavel")
                    .SetProperty(a => a.LastModificationDateTime, assinaturaEm),
                    cancellationToken);

            if (updatedRows == 0)
            {
                var exists = await context.AplicacoesVacina
                    .AsNoTracking()
                    .AnyAsync(a => a.Id == id, cancellationToken);

                if (!exists)
                    return null;

                throw new InvalidOperationException(
                    "Esta aplicação já foi assinada ou foi alterada por outra requisição. Recarregue a lista e tente novamente.");
            }

            context.AssinaturasAplicacoesVacina.Add(new AssinaturaAplicacaoVacina
            {
                Id = Guid.NewGuid(),
                AplicacaoVacinaId = id,
                TipoEvento = "Assinatura",
                NomeAssinante = nome,
                Crmv = crmv,
                CrmvUf = uf,
                DataHora = assinaturaEm,
                HashConteudo = hash,
                Metodo = "InternaAuditavel",
                Observacoes = string.IsNullOrWhiteSpace(dto.Observacoes) ? null : dto.Observacoes.Trim()
            });

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return aplicacao;
    }

    public async Task<AssinaturaValidacaoResponseDto?> ValidarAssinaturaAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var aplicacao = await context.AplicacoesVacina
            .AsNoTracking()
            .Include(a => a.Assinaturas)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (aplicacao is null)
            return null;

        if (aplicacao.StatusComprovante != StatusComprovanteVacina.Assinado)
        {
            return new AssinaturaValidacaoResponseDto
            {
                AplicacaoVacinaId = id,
                Assinada = false,
                Integra = false,
                AuditoriaIntegra = false,
                AssinadaEm = aplicacao.AssinadaEm,
                HashArmazenado = aplicacao.HashAssinatura,
                Mensagem = "A aplicação ainda não foi assinada."
            };
        }

        var requiredSignatureDataIsPresent = aplicacao.AssinadaEm.HasValue &&
                                             !string.IsNullOrWhiteSpace(aplicacao.Assinante) &&
                                             !string.IsNullOrWhiteSpace(aplicacao.AssinanteCrmv) &&
                                             !string.IsNullOrWhiteSpace(aplicacao.AssinanteCrmvUf) &&
                                             !string.IsNullOrWhiteSpace(aplicacao.HashAssinatura);

        if (!requiredSignatureDataIsPresent)
        {
            return new AssinaturaValidacaoResponseDto
            {
                AplicacaoVacinaId = id,
                Assinada = true,
                Integra = false,
                AuditoriaIntegra = false,
                AssinadaEm = aplicacao.AssinadaEm,
                HashArmazenado = aplicacao.HashAssinatura,
                Mensagem = "A aplicação está marcada como assinada, mas os dados da assinatura estão incompletos."
            };
        }

        var assinaturaEm = aplicacao.AssinadaEm;
        var assinaturaUf = NormalizeUf(aplicacao.AssinanteCrmvUf);
        if (string.IsNullOrWhiteSpace(assinaturaUf))
        {
            return new AssinaturaValidacaoResponseDto
            {
                AplicacaoVacinaId = id,
                Assinada = true,
                Integra = false,
                AuditoriaIntegra = false,
                AssinadaEm = aplicacao.AssinadaEm,
                HashArmazenado = aplicacao.HashAssinatura,
                Mensagem = "A aplicação está marcada como assinada, mas a UF do CRMV é inválida."
            };
        }

        var hashCalculado = CalculateSignatureHash(
            aplicacao,
            aplicacao.Assinante!.Trim(),
            aplicacao.AssinanteCrmv!.Trim(),
            assinaturaUf,
            assinaturaEm!.Value);

        var hashMatches = string.Equals(aplicacao.HashAssinatura, hashCalculado,
            StringComparison.OrdinalIgnoreCase);

        // Compatibilidade com assinaturas emitidas antes da canonicalização do
        // timestamp. O banco pode ter removido até 9 tiques de 100 ns do valor
        // usado originalmente no hash, sem alterar o instante persistido.
        if (!hashMatches)
        {
            for (var ticks = -9; ticks <= 9; ticks++)
            {
                var legacyHash = CalculateSignatureHash(
                    aplicacao,
                    aplicacao.Assinante.Trim(),
                    aplicacao.AssinanteCrmv.Trim(),
                    assinaturaUf,
                    assinaturaEm.Value.AddTicks(ticks));

                if (!string.Equals(aplicacao.HashAssinatura, legacyHash,
                        StringComparison.OrdinalIgnoreCase))
                    continue;

                hashCalculado = legacyHash;
                hashMatches = true;
                break;
            }
        }

        var auditEvent = aplicacao.Assinaturas
            .Where(a => a.TipoEvento == "Assinatura")
            .OrderByDescending(a => a.DataHora)
            .FirstOrDefault();

        var auditMatches = auditEvent is not null &&
                           string.Equals(auditEvent.HashConteudo, aplicacao.HashAssinatura,
                               StringComparison.OrdinalIgnoreCase) &&
                           string.Equals(auditEvent.NomeAssinante, aplicacao.Assinante,
                               StringComparison.Ordinal) &&
                           string.Equals(auditEvent.Crmv, aplicacao.AssinanteCrmv,
                               StringComparison.Ordinal) &&
                           string.Equals(auditEvent.CrmvUf, aplicacao.AssinanteCrmvUf,
                               StringComparison.OrdinalIgnoreCase) &&
                           auditEvent.DataHora == assinaturaEm.Value;

        return new AssinaturaValidacaoResponseDto
        {
            AplicacaoVacinaId = id,
            Assinada = true,
            Integra = hashMatches && auditMatches,
            AuditoriaIntegra = auditMatches,
            AssinadaEm = aplicacao.AssinadaEm,
            HashArmazenado = aplicacao.HashAssinatura,
            HashCalculado = hashCalculado,
            Mensagem = hashMatches && auditMatches
                ? "Assinatura íntegra e registro de auditoria conferido."
                : "A assinatura não confere com o conteúdo atual da aplicação ou com seu registro de auditoria."
        };
    }

    private async Task<(AnimalProtocoloVacinal Assignment, ProtocoloVacinalDose Dose)?> ValidateProtocolLinkAsync(
        Guid animalId,
        Guid vacinaId,
        Guid? animalProtocolId,
        Guid? protocolDoseId,
        CancellationToken cancellationToken)
    {
        if (!animalProtocolId.HasValue && !protocolDoseId.HasValue)
            return null;

        if (!animalProtocolId.HasValue || !protocolDoseId.HasValue)
            throw new ArgumentException("A aplicação vinculada a um protocolo deve informar o protocolo do animal e a dose.");

        var assignment = await context.AnimalProtocolosVacinais
            .Include(p => p.ProtocoloVacinal)
                .ThenInclude(p => p!.Doses)
            .FirstOrDefaultAsync(p => p.Id == animalProtocolId.Value, cancellationToken)
            ?? throw new KeyNotFoundException($"Protocolo do animal com Id '{animalProtocolId}' não encontrado.");

        if (assignment.AnimalId != animalId)
            throw new ArgumentException("O protocolo informado não pertence ao animal da aplicação.");

        var dose = assignment.ProtocoloVacinal?.Doses.FirstOrDefault(d => d.Id == protocolDoseId.Value)
            ?? throw new KeyNotFoundException($"Dose de protocolo com Id '{protocolDoseId}' não encontrada.");

        if (dose.VacinaId != vacinaId)
            throw new ArgumentException("A vacina da aplicação não corresponde à vacina da dose do protocolo.");

        return (assignment, dose);
    }

    private static DateOnly? CalculateNextDoseDate(AnimalProtocoloVacinal assignment,
        ProtocoloVacinalDose dose, DateOnly applicationDate)
    {
        if (dose.IntervaloReforcoDias.HasValue)
            return applicationDate.AddDays(dose.IntervaloReforcoDias.Value);

        var nextDose = assignment.ProtocoloVacinal?.Doses
            .Where(d => d.Ordem > dose.Ordem)
            .OrderBy(d => d.Ordem)
            .FirstOrDefault();

        return nextDose is null ? null : assignment.DataInicio.AddDays(nextDose.DiasAposInicio);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var aplicacao =
            await context.AplicacoesVacina.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (aplicacao is null)
        {
            return false;
        }

        EnsureNotSigned(aplicacao);

        context.AplicacoesVacina.Remove(aplicacao);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<AplicacaoVacina?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.AplicacoesVacina
            .AsNoTracking()
            .Include(aplicacao => aplicacao.Vacina)
                .ThenInclude(v => v!.Especie)
            .Include(aplicacao => aplicacao.Animal)
                .ThenInclude(a => a!.Identificadores)
            .Include(aplicacao => aplicacao.Animal)
                .ThenInclude(a => a!.Raca)
            .Include(aplicacao => aplicacao.Assinaturas)
            .FirstOrDefaultAsync(aplicacao => aplicacao.Id == id, cancellationToken);
    }

    public Task<List<AplicacaoVacina>> GetAllAsync(int? page, AplicacaoVacinaSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.AplicacoesVacina
            .Include(aplicacao => aplicacao.Vacina)
            .Include(aplicacao => aplicacao.Animal)
                .ThenInclude(a => a!.Identificadores)
            .AsNoTracking();

        query = ApplyFilters(query, search, null);

        var currentPage = page ?? 1;
        query = query.OrderByDescending(aplicacao => aplicacao.DataAplicacao)
            .Skip((currentPage - 1) * 10).Take(10);

        return query.ToListAsync(cancellationToken);
    }

    public Task<int> Count(AplicacaoVacinaSearchDto? search = null, bool? somenteAtrasadas = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.AplicacoesVacina.AsNoTracking();
        query = ApplyFilters(query, search, somenteAtrasadas);
        return query.CountAsync(cancellationToken);
    }

    private IQueryable<AplicacaoVacina> ApplyFilters(IQueryable<AplicacaoVacina> query, AplicacaoVacinaSearchDto? search, bool? somenteAtrasadas)
    {
        if (somenteAtrasadas == true || search?.SomenteAtrasadas == true)
        {
            var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
            query = query.Where(a => !a.CicloFinalizado && a.DataProximaDose != null && a.DataProximaDose < hoje);
            query = query.Where(a => !context.AplicacoesVacina.Any(posterior =>
                posterior.AnimalId == a.AnimalId &&
                posterior.VacinaId == a.VacinaId &&
                posterior.DataAplicacao > a.DataAplicacao));
        }

        if (search is null)
            return query;

        if (search.VacinaId.HasValue)
            query = query.Where(aplicacao => aplicacao.VacinaId == search.VacinaId.Value);

        if (search.AnimalId.HasValue)
            query = query.Where(aplicacao => aplicacao.AnimalId == search.AnimalId.Value);

        if (search.AnimalProtocoloVacinalId.HasValue)
            query = query.Where(aplicacao => aplicacao.AnimalProtocoloVacinalId == search.AnimalProtocoloVacinalId.Value);

        if (search.ProtocoloVacinalDoseId.HasValue)
            query = query.Where(aplicacao => aplicacao.ProtocoloVacinalDoseId == search.ProtocoloVacinalDoseId.Value);

        if (search.ProtocoloVacinalId.HasValue)
            query = query.Where(aplicacao => aplicacao.AnimalProtocoloVacinal != null &&
                                             aplicacao.AnimalProtocoloVacinal.ProtocoloVacinalId == search.ProtocoloVacinalId.Value);

        if (search.DataAplicacaoFrom.HasValue)
            query = query.Where(aplicacao => aplicacao.DataAplicacao >= search.DataAplicacaoFrom.Value);

        if (search.DataAplicacaoTo.HasValue)
            query = query.Where(aplicacao => aplicacao.DataAplicacao <= search.DataAplicacaoTo.Value);

        if (search.DataProximaDoseFrom.HasValue)
            query = query.Where(aplicacao => aplicacao.DataProximaDose >= search.DataProximaDoseFrom.Value);

        if (search.DataProximaDoseTo.HasValue)
            query = query.Where(aplicacao => aplicacao.DataProximaDose <= search.DataProximaDoseTo.Value);

        if (search.DataLimite.HasValue)
            query = query.Where(aplicacao => aplicacao.DataProximaDose != null && aplicacao.DataProximaDose <= search.DataLimite.Value);

        if (search.CicloFinalizado.HasValue)
            query = query.Where(aplicacao => aplicacao.CicloFinalizado == search.CicloFinalizado.Value);

        if (search.StatusComprovante.HasValue)
            query = query.Where(aplicacao => aplicacao.StatusComprovante == search.StatusComprovante.Value);

        if (search.PendenteAssinatura == true)
        {
            query = query.Where(a => a.StatusComprovante == StatusComprovanteVacina.NaoEmitido ||
                                     a.StatusComprovante == StatusComprovanteVacina.PendenteAssinatura);
        }

        if (search.Proximas == true)
        {
            var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
            var limite = search.DataLimite ?? hoje.AddDays(30);
            query = query.Where(a => a.DataProximaDose != null && a.DataProximaDose >= hoje && a.DataProximaDose <= limite);
        }

        return query;
    }

    public Task<List<AplicacaoVacina>> GetAtrasadasAsync(int? page, Guid? animalId, Guid? vacinaId,
        StatusComprovanteVacina? statusComprovante, CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = context.AplicacoesVacina
            .Include(aplicacao => aplicacao.Vacina)
            .Include(aplicacao => aplicacao.Animal)
                .ThenInclude(a => a!.Identificadores)
            .AsNoTracking()
            .Where(a => !a.CicloFinalizado && a.DataProximaDose != null && a.DataProximaDose < hoje);

        query = query.Where(a => !context.AplicacoesVacina.Any(posterior =>
            posterior.AnimalId == a.AnimalId &&
            posterior.VacinaId == a.VacinaId &&
            posterior.DataAplicacao > a.DataAplicacao));

        if (animalId.HasValue)
            query = query.Where(a => a.AnimalId == animalId.Value);

        if (vacinaId.HasValue)
            query = query.Where(a => a.VacinaId == vacinaId.Value);

        if (statusComprovante.HasValue)
            query = query.Where(a => a.StatusComprovante == statusComprovante.Value);

        var currentPage = page ?? 1;
        query = query.OrderBy(a => a.DataProximaDose)
            .Skip((currentPage - 1) * 10)
            .Take(10);

        return query.ToListAsync(cancellationToken);
    }

    public Task<int> CountAtrasadasAsync(Guid? animalId = null, Guid? vacinaId = null,
        StatusComprovanteVacina? statusComprovante = null, CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = context.AplicacoesVacina
            .AsNoTracking()
            .Where(a => !a.CicloFinalizado && a.DataProximaDose != null && a.DataProximaDose < hoje);

        query = query.Where(a => !context.AplicacoesVacina.Any(posterior =>
            posterior.AnimalId == a.AnimalId &&
            posterior.VacinaId == a.VacinaId &&
            posterior.DataAplicacao > a.DataAplicacao));

        if (animalId.HasValue)
            query = query.Where(a => a.AnimalId == animalId.Value);

        if (vacinaId.HasValue)
            query = query.Where(a => a.VacinaId == vacinaId.Value);

        if (statusComprovante.HasValue)
            query = query.Where(a => a.StatusComprovante == statusComprovante.Value);

        return query.CountAsync(cancellationToken);
    }

    public Task<List<AplicacaoVacina>> GetPendentesAssinaturaAsync(int? page, Guid? animalId, Guid? vacinaId,
        StatusComprovanteVacina? statusComprovante, CancellationToken cancellationToken = default)
    {
        var query = context.AplicacoesVacina
            .Include(aplicacao => aplicacao.Vacina)
            .Include(aplicacao => aplicacao.Animal)
                .ThenInclude(a => a!.Identificadores)
            .AsNoTracking();

        if (statusComprovante.HasValue)
        {
            query = query.Where(a => a.StatusComprovante == statusComprovante.Value);
        }
        else
        {
            query = query.Where(a => a.StatusComprovante == StatusComprovanteVacina.NaoEmitido ||
                                     a.StatusComprovante == StatusComprovanteVacina.PendenteAssinatura);
        }

        if (animalId.HasValue)
            query = query.Where(a => a.AnimalId == animalId.Value);

        if (vacinaId.HasValue)
            query = query.Where(a => a.VacinaId == vacinaId.Value);

        var currentPage = page ?? 1;
        query = query.OrderBy(a => a.DataAplicacao)
            .Skip((currentPage - 1) * 10)
            .Take(10);

        return query.ToListAsync(cancellationToken);
    }

    public Task<int> CountPendentesAssinaturaAsync(Guid? animalId = null, Guid? vacinaId = null,
        StatusComprovanteVacina? statusComprovante = null, CancellationToken cancellationToken = default)
    {
        var query = context.AplicacoesVacina
            .AsNoTracking();

        if (statusComprovante.HasValue)
        {
            query = query.Where(a => a.StatusComprovante == statusComprovante.Value);
        }
        else
        {
            query = query.Where(a => a.StatusComprovante == StatusComprovanteVacina.NaoEmitido ||
                                     a.StatusComprovante == StatusComprovanteVacina.PendenteAssinatura);
        }

        if (animalId.HasValue)
            query = query.Where(a => a.AnimalId == animalId.Value);

        if (vacinaId.HasValue)
            query = query.Where(a => a.VacinaId == vacinaId.Value);

        return query.CountAsync(cancellationToken);
    }

    public Task<List<AplicacaoVacina>> GetProximasAsync(int? page, DateOnly? dataLimite, Guid? animalId,
        Guid? vacinaId, CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var limite = dataLimite ?? hoje.AddDays(30);

        var query = context.AplicacoesVacina
            .Include(aplicacao => aplicacao.Vacina)
            .Include(aplicacao => aplicacao.Animal)
                .ThenInclude(a => a!.Identificadores)
            .AsNoTracking()
            .Where(a => a.DataProximaDose != null && a.DataProximaDose >= hoje && a.DataProximaDose <= limite);

        if (animalId.HasValue)
            query = query.Where(a => a.AnimalId == animalId.Value);

        if (vacinaId.HasValue)
            query = query.Where(a => a.VacinaId == vacinaId.Value);

        var currentPage = page ?? 1;
        query = query.OrderBy(a => a.DataProximaDose)
            .Skip((currentPage - 1) * 10)
            .Take(10);

        return query.ToListAsync(cancellationToken);
    }

    public Task<int> CountProximasAsync(DateOnly? dataLimite = null, Guid? animalId = null,
        Guid? vacinaId = null, CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var limite = dataLimite ?? hoje.AddDays(30);

        var query = context.AplicacoesVacina
            .AsNoTracking()
            .Where(a => a.DataProximaDose != null && a.DataProximaDose >= hoje && a.DataProximaDose <= limite);

        if (animalId.HasValue)
            query = query.Where(a => a.AnimalId == animalId.Value);

        if (vacinaId.HasValue)
            query = query.Where(a => a.VacinaId == vacinaId.Value);

        return query.CountAsync(cancellationToken);
    }

    public async Task<List<AplicacaoVacina>> VacinarLoteAsync(AplicacaoVacinaLoteCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var currentUser = GetAuthenticatedUser();
        var vacina = await context.Vacinas.FindAsync([dto.VacinaId], cancellationToken);
        if (vacina is null)
            throw new KeyNotFoundException($"Vacina com Id '{dto.VacinaId}' não encontrada.");

        var dataProximaDose = dto.DataProximaDose;
        if (dataProximaDose is null && vacina.ReaplicarEmXDias > 0)
        {
            dataProximaDose = dto.DataAplicacao.AddDays((int)vacina.ReaplicarEmXDias);
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var criadas = new List<AplicacaoVacina>();

            foreach (var item in dto.Animais)
            {
                var animalExists = await context.Animals.AnyAsync(a => a.Id == item.AnimalId, cancellationToken);
                if (!animalExists)
                {
                    throw new KeyNotFoundException($"Animal com Id '{item.AnimalId}' não encontrado.");
                }

                var aplicacao = new AplicacaoVacina
                {
                    Id = Guid.NewGuid(),
                    AnimalId = item.AnimalId,
                    VacinaId = dto.VacinaId,
                    DataAplicacao = dto.DataAplicacao,
                    DataProximaDose = dataProximaDose,
                    CicloFinalizado = item.CicloFinalizado,
                    StatusComprovante = StatusComprovanteVacina.PendenteAssinatura,
                    NumeroLote = dto.NumeroLote,
                    DoseMl = dto.DoseMl,
                    DataFabricacao = dto.DataFabricacao,
                    DataValidade = dto.DataValidade,
                    LocalAplicacao = dto.LocalAplicacao,
                    Observacoes = dto.Observacoes,
                    VeterinarioResponsavel = currentUser.Name,
                    VeterinarioCrmv = currentUser.Crmv,
                    VeterinarioCrmvUf = currentUser.CrmvUf,
                    Aplicador = currentUser.Name,
                    LaboratorioFabricante = dto.LaboratorioFabricante
                };

                context.AplicacoesVacina.Add(aplicacao);
                criadas.Add(aplicacao);
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return criadas;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ComprovanteVacinasDadosResponseDto?> GetComprovanteVacinasDadosAsync(Guid animalId,
        bool incluirNaoAssinadas, CancellationToken cancellationToken = default)
    {
        var animal = await context.Animals
            .AsNoTracking()
            .Include(a => a.Raca)
                .ThenInclude(r => r!.Especie)
            .Include(a => a.Identificadores)
            .FirstOrDefaultAsync(a => a.Id == animalId, cancellationToken);

        if (animal is null)
            return null;

        var aplicacoesQuery = context.AplicacoesVacina
            .AsNoTracking()
            .Include(a => a.Vacina)
                .ThenInclude(v => v!.Especie)
            .Include(a => a.Assinaturas)
            .Where(a => a.AnimalId == animalId);

        if (!incluirNaoAssinadas)
            aplicacoesQuery = aplicacoesQuery.Where(a => a.StatusComprovante == StatusComprovanteVacina.Assinado);

        var aplicacoes = await aplicacoesQuery
            .OrderBy(a => a.DataAplicacao)
            .ThenBy(a => a.CreationDateTime)
            .ToListAsync(cancellationToken);

        return new ComprovanteVacinasDadosResponseDto
        {
            Animal = AnimalMapper.MapToResponse(animal),
            AplicacoesVacina = aplicacoes.Select(AplicacaoVacinaMapper.MapToResponse).ToList(),
            GeradoEm = DateTimeOffset.UtcNow
        };
    }

    private static void EnsureNotSigned(AplicacaoVacina aplicacao)
    {
        if (aplicacao.StatusComprovante == StatusComprovanteVacina.Assinado)
            throw new InvalidOperationException("Aplicações assinadas não podem ser alteradas. Registre uma nova aplicação para corrigir o histórico.");
    }

    private CurrentUser GetAuthenticatedUser()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        var name = principal?.FindFirstValue("name")
            ?? principal?.FindFirstValue("display_name")
            ?? principal?.FindFirstValue(ClaimTypes.Name)
            ?? principal?.FindFirstValue("preferred_username");

        if (principal?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Não foi possível identificar o usuário autenticado.");

        var crmv = principal.FindFirstValue("crmv")?.Trim();
        var crmvUf = NormalizeUf(principal.FindFirstValue("crmv_uf"));
        if (principal.IsInRole("medico_veterinario") &&
            (string.IsNullOrWhiteSpace(crmv) || string.IsNullOrWhiteSpace(crmvUf)))
        {
            throw new ArgumentException("Cadastre o CRMV e a UF do CRMV do usuário antes de registrar uma vacinação.");
        }

        return new CurrentUser(name.Trim(), string.IsNullOrWhiteSpace(crmv) ? null : crmv, crmvUf);
    }

    private sealed record CurrentUser(string Name, string? Crmv, string? CrmvUf);

    private static string? NormalizeUf(string? uf)
    {
        var normalized = uf?.Trim().ToUpperInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static DateTimeOffset NormalizeSignatureTimestamp(DateTimeOffset value)
    {
        var utc = value.ToUniversalTime();
        return new DateTimeOffset(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, 0,
            TimeSpan.Zero);
    }

    private static void ValidateVaccineDates(DateOnly? dataFabricacao, DateOnly? dataValidade)
    {
        if (dataFabricacao.HasValue && dataValidade.HasValue && dataValidade.Value < dataFabricacao.Value)
            throw new ArgumentException("A data de validade não pode ser anterior à data de fabricação.");
    }

    private static string CalculateSignatureHash(AplicacaoVacina aplicacao, string nome, string crmv,
        string uf, DateTimeOffset assinaturaEm)
    {
        var canonical = string.Join("|", new[]
        {
            aplicacao.Id.ToString("N"),
            aplicacao.AnimalId.ToString("N"),
            aplicacao.VacinaId.ToString("N"),
            aplicacao.DataAplicacao.ToString("yyyy-MM-dd"),
            aplicacao.DataProximaDose?.ToString("yyyy-MM-dd") ?? string.Empty,
            aplicacao.NumeroLote.Trim(),
            aplicacao.DoseMl?.Trim() ?? string.Empty,
            aplicacao.DataFabricacao?.ToString("yyyy-MM-dd") ?? string.Empty,
            aplicacao.DataValidade?.ToString("yyyy-MM-dd") ?? string.Empty,
            aplicacao.LocalAplicacao?.Trim() ?? string.Empty,
            aplicacao.LaboratorioFabricante?.Trim() ?? string.Empty,
            aplicacao.VeterinarioResponsavel?.Trim() ?? string.Empty,
            aplicacao.VeterinarioCrmv?.Trim() ?? string.Empty,
            aplicacao.VeterinarioCrmvUf?.Trim() ?? string.Empty,
            aplicacao.Aplicador?.Trim() ?? string.Empty,
            aplicacao.Observacoes?.Trim() ?? string.Empty,
            nome,
            crmv,
            uf,
            assinaturaEm.ToUniversalTime().ToString("O")
        });

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }
}
