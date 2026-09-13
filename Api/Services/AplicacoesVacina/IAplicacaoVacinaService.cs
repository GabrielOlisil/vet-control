using Api.DTOs;
using Api.Models;
using Api.Models.Enums;

namespace Api.Services.AplicacoesVacina;

public interface IAplicacaoVacinaService
{
    Task<AplicacaoVacina> CreateAsync(AplicacaoVacinaCreateDto dto,
        CancellationToken cancellationToken = default);

    Task<AplicacaoVacina?> PatchAsync(Guid id, AplicacaoVacinaPatchDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AplicacaoVacina?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<AplicacaoVacina>> GetAllAsync(int? page, AplicacaoVacinaSearchDto? search = null,
        CancellationToken cancellationToken = default);

    Task<int> Count(AplicacaoVacinaSearchDto? search = null, bool? somenteAtrasadas = null,
        CancellationToken cancellationToken = default);

    Task<List<AplicacaoVacina>> GetAtrasadasAsync(int? page, Guid? animalId, Guid? vacinaId,
        StatusComprovanteVacina? statusComprovante, CancellationToken cancellationToken = default);

    Task<int> CountAtrasadasAsync(Guid? animalId = null, Guid? vacinaId = null,
        StatusComprovanteVacina? statusComprovante = null, CancellationToken cancellationToken = default);

    Task<List<AplicacaoVacina>> GetPendentesAssinaturaAsync(int? page, Guid? animalId, Guid? vacinaId,
        StatusComprovanteVacina? statusComprovante, CancellationToken cancellationToken = default);

    Task<int> CountPendentesAssinaturaAsync(Guid? animalId = null, Guid? vacinaId = null,
        StatusComprovanteVacina? statusComprovante = null, CancellationToken cancellationToken = default);

    Task<List<AplicacaoVacina>> GetProximasAsync(int? page, DateOnly? dataLimite, Guid? animalId,
        Guid? vacinaId, CancellationToken cancellationToken = default);

    Task<int> CountProximasAsync(DateOnly? dataLimite = null, Guid? animalId = null,
        Guid? vacinaId = null, CancellationToken cancellationToken = default);

    Task<List<AplicacaoVacina>> VacinarLoteAsync(AplicacaoVacinaLoteCreateDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> UploadComprovanteAsync(Guid aplicacaoId, Stream streamArquivo, string contentType, CancellationToken ct);
    Task<(byte[] Bytes, string ContentType)?> GetComprovanteAsync(Guid aplicacaoId, CancellationToken ct);
}