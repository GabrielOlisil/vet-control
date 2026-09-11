using Api.DTOs;
using Api.Models;

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

    Task<int> Count(AplicacaoVacinaSearchDto? search = null, CancellationToken cancellationToken = default);

    Task<bool> UploadComprovanteAsync(Guid aplicacaoId, Stream streamArquivo, string contentType, CancellationToken ct);
    Task<(byte[] Bytes, string ContentType)?> GetComprovanteAsync(Guid aplicacaoId, CancellationToken ct);
}