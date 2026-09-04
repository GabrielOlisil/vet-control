using Api.DTOs.Vacinas;
using Api.DTOs;
using Api.Models;

namespace Api.Services.Vacinas;

public interface IVacinaService
{
    Task<List<Vacina>> GetAllAsync(VacinaSearchDto? search = null,
        CancellationToken cancellationToken = default);

    Task<Vacina?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Vacina> CreateAsync(VacinaCreateDto dto, CancellationToken cancellationToken = default);
    Task<Vacina?> PatchAsync(Guid id, VacinaPatchDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}