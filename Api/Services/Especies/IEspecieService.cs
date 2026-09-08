using Api.DTOs.Especies;
using Api.DTOs;
using Api.Models;

namespace Api.Services.Especies;

public interface IEspecieService
{
    Task<List<Especie>> GetAllAsync(int? page, EspecieSearchDto? search = null, CancellationToken cancellationToken = default);
    Task<Especie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Especie> CreateAsync(EspecieCreateDto dto, CancellationToken cancellationToken = default);
    Task<Especie?> PatchAsync(Guid id, EspeciePatchDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> Count(EspecieSearchDto? search = null, CancellationToken cancellationToken = default);
}
