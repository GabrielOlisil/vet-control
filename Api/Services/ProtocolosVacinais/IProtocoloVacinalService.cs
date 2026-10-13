using Api.DTOs;
using Api.Models;

namespace Api.Services.ProtocolosVacinais;

public interface IProtocoloVacinalService
{
    Task<List<ProtocoloVacinal>> GetAllAsync(int? page, ProtocoloVacinalSearchDto? search = null,
        CancellationToken cancellationToken = default);
    Task<ProtocoloVacinal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProtocoloVacinal> CreateAsync(ProtocoloVacinalCreateDto dto, CancellationToken cancellationToken = default);
    Task<ProtocoloVacinal?> PatchAsync(Guid id, ProtocoloVacinalPatchDto dto,
        CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(ProtocoloVacinalSearchDto? search = null, CancellationToken cancellationToken = default);

    Task<AnimalProtocoloVacinal?> GetAnimalProtocolByIdAsync(Guid animalProtocolId,
        CancellationToken cancellationToken = default);
    Task<List<AnimalProtocoloVacinal>> GetAnimalProtocolsAsync(Guid animalId,
        CancellationToken cancellationToken = default);
    Task<AnimalProtocoloVacinal> AssignToAnimalAsync(Guid animalId, AnimalProtocoloVacinalCreateDto dto,
        CancellationToken cancellationToken = default);
    Task<AnimalProtocoloVacinal?> PatchAnimalProtocolAsync(Guid animalProtocolId,
        AnimalProtocoloVacinalPatchDto dto, CancellationToken cancellationToken = default);

    Task<List<ProtocoloVacinalPendenciaResponseDto>> GetPendenciasAsync(int? page,
        ProtocoloVacinalPendenciaSearchDto? search = null, CancellationToken cancellationToken = default);
    Task<int> CountPendenciasAsync(ProtocoloVacinalPendenciaSearchDto? search = null,
        CancellationToken cancellationToken = default);
}
