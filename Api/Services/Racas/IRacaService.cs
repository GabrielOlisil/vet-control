using Api.DTOs;
using Api.Models;

namespace Api.Services.Racas;

public interface IRacaService
{
    Task<List<Raca>> GetAllAsync(RacaSearchDto? search = null,
        CancellationToken cancellationToken = default);

    Task<Raca?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Raca> CreateAsync(RacaCreateDto dto, CancellationToken cancellationToken = default);
    Task<Raca?> PatchAsync(Guid id, RacaPatchDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> Count(CancellationToken cancellationToken);
    Task<List<Raca>> GetAllByNameAsync(int page, string name, CancellationToken cancellationToken = default);

}
