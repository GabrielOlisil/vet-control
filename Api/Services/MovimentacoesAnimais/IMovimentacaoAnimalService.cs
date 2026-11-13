using Api.DTOs;
using Api.Models;

namespace Api.Services.MovimentacoesAnimais;

public interface IMovimentacaoAnimalService
{
    Task<List<MovimentacaoAnimal>> GetAllAsync(int? page, MovimentacaoAnimalSearchDto? search = null,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(MovimentacaoAnimalSearchDto? search = null,
        CancellationToken cancellationToken = default);
    Task<MovimentacaoAnimal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MovimentacaoAnimal> CreateAsync(Guid animalId, MovimentacaoAnimalCreateDto dto,
        CancellationToken cancellationToken = default);
    Task<List<MovimentacaoAnimal>> CreateManyAsync(MovimentacaoAnimalLoteCreateDto dto,
        CancellationToken cancellationToken = default);
}
