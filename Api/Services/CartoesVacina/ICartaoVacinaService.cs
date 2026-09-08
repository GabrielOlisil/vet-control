using Api.DTOs.CartoesVacina;
using Api.DTOs;
using Api.Models;

namespace Api.Services.CartoesVacina;

public interface ICartaoVacinaService
{
    Task<List<CartaoVacina>> GetAllAsync(CartaoVacinaSearchDto? search = null,
        CancellationToken cancellationToken = default);

    Task<CartaoVacina?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CartaoVacina> CreateAsync(CartaoVacinaCreateDto dto, CancellationToken cancellationToken = default);

    Task<CartaoVacina?> PatchAsync(Guid id, CartaoVacinaPatchDto dto,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> Count(CancellationToken cancellationToken);
}