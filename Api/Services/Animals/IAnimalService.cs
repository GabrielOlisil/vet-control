using Api.DTOs.Animals;
using Api.DTOs;
using Api.Models;

namespace Api.Services.Animals;

public interface IAnimalService
{
    Task<Animal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Animal>> GetAllAsync(int? page, AnimalSearchDto? search = null,
        CancellationToken cancellationToken = default);

    Task<Animal> CreateAsync(AnimalCreateDto dto, CancellationToken cancellationToken = default);
    Task<Animal?> PatchAsync(Guid id, AnimalPatchDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> Count(AnimalSearchDto? search = null, CancellationToken cancellationToken = default);

    Task<List<Animal>> GetAllByNameAsync(int page, string name, CancellationToken cancellationToken = default);

    Task<AnimalProntuarioResponseDto?> GetProntuarioAsync(Guid animalId, CancellationToken ct);
}