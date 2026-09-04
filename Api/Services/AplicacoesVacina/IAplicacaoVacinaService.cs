using Api.DTOs.AplicacoesVacina;
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
    Task<List<AplicacaoVacina>> GetAllAsync(AplicacaoVacinaSearchDto? search = null,
        CancellationToken cancellationToken = default);
}