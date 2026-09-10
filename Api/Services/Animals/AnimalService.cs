using Api.Database;
using Api.DTOs.Animals;
using Api.DTOs.AplicacoesVacina;
using Api.DTOs;
using Api.Mappers;
using Api.Models;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Animals;

public sealed class AnimalService(ApiContext context) : IAnimalService
{
    public Task<Animal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Animals
            .AsNoTracking()
            .Include(a => a.Raca)
                .ThenInclude(r => r!.Especie)
            .Include(a => a.Identificadores)
            .FirstOrDefaultAsync(animal => animal.Id == id, cancellationToken);
    }

    public Task<List<Animal>> GetAllAsync(int? page, AnimalSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Animals
            .Include(e => e.Raca)
            .Include(a => a.Identificadores)
            .AsNoTracking();

        if (search?.RacaId is not null)
            query = query.Where(animal => animal.RacaId == search.RacaId);
        if (search?.DataNascimentoFrom is not null)
            query = query.Where(animal => animal.DataNascimento >= search.DataNascimentoFrom);
        if (search?.DataNascimentoTo is not null)
            query = query.Where(animal => animal.DataNascimento <= search.DataNascimentoTo);

        query = query.OrderBy(animal => animal.Name).ThenBy(animal => animal.CreationDateTime)
                    .ThenBy(animal => animal.Id);

        if (!page.HasValue)
        {
            page = 1;
        }
        query = query.Skip((page.Value - 1) * 10).Take(10);

        return query.ToListAsync(cancellationToken);
    }

    public async Task<Animal> CreateAsync(AnimalCreateDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Identificadores.Count == 0)
            throw new ArgumentException("Ao menos um identificador é obrigatório.");

        // Se nenhum identificador vier marcado como principal, marcar o primeiro
        if (!dto.Identificadores.Any(i => i.EhPrincipal))
        {
            dto.Identificadores[0] = dto.Identificadores[0] with { EhPrincipal = true };
        }

        var animal = new Animal
        {
            Id = Guid.NewGuid(),
            Name = dto.Name ?? string.Empty,
            RacaId = dto.RacaId,
            DataNascimento = dto.DataNascimento,
            Sexo = dto.Sexo,
            OrigemAnimal = dto.Origem,
            LoteOuPasto = dto.LoteOuPasto,
            Identificadores = dto.Identificadores.Select(i => new IdentificadorAnimal
            {
                Id = Guid.NewGuid(),
                Tipo = i.Tipo,
                Valor = i.Valor,
                IsPrincipal = i.EhPrincipal
            }).ToList()
        };

        context.Animals.Add(animal);
        await context.SaveChangesAsync(cancellationToken);

        return animal;
    }

    public async Task<Animal?> PatchAsync(Guid id, AnimalPatchDto dto,
        CancellationToken cancellationToken = default)
    {
        var animal = await context.Animals
            .Include(a => a.Identificadores)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (animal is null)
        {
            return null;
        }

        if (dto.Name is not null)
        {
            animal.Name = dto.Name;
        }

        if (dto.DataNascimento.HasValue)
        {
            animal.DataNascimento = dto.DataNascimento.Value;
        }

        if (dto.RacaId.HasValue)
        {
            animal.RacaId = dto.RacaId.Value;
        }

        if (dto.Sexo.HasValue)
        {
            animal.Sexo = dto.Sexo.Value;
        }

        if (dto.Origem.HasValue)
        {
            animal.OrigemAnimal = dto.Origem.Value;
        }

        if (dto.LoteOuPasto is not null)
        {
            animal.LoteOuPasto = dto.LoteOuPasto;
        }

        if (dto.Identificadores is not null)
        {
            // Replace all identificadores
            context.IdentificadorAnimal.RemoveRange(animal.Identificadores);

            if (!dto.Identificadores.Any(i => i.EhPrincipal) && dto.Identificadores.Count > 0)
            {
                dto.Identificadores[0] = dto.Identificadores[0] with { EhPrincipal = true };
            }

            animal.Identificadores = dto.Identificadores.Select(i => new IdentificadorAnimal
            {
                Id = Guid.NewGuid(),
                AnimalId = animal.Id,
                Tipo = i.Tipo,
                Valor = i.Valor,
                IsPrincipal = i.EhPrincipal
            }).ToList();
        }

        await context.SaveChangesAsync(cancellationToken);

        return animal;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var animal = await context.Animals.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (animal is null)
        {
            return false;
        }

        context.Animals.Remove(animal);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<int> Count(AnimalSearchDto? search = null, CancellationToken cancellationToken = default)
    {
        var query = context.Animals.AsNoTracking();

        if (search?.RacaId is not null)
            query = query.Where(animal => animal.RacaId == search.RacaId);
        if (search?.DataNascimentoFrom is not null)
            query = query.Where(animal => animal.DataNascimento >= search.DataNascimentoFrom);
        if (search?.DataNascimentoTo is not null)
            query = query.Where(animal => animal.DataNascimento <= search.DataNascimentoTo);

        return query.CountAsync(cancellationToken);
    }

    public Task<List<Animal>> GetAllByNameAsync(int page, string name, CancellationToken cancellationToken = default)
    {
        return context.Animals
            .AsNoTracking()
            .Include(a => a.Identificadores)
            .Where(e => EF.Functions.ILike(e.Name ?? string.Empty, $"{name}%")
                || e.Identificadores.Any(i => EF.Functions.ILike(i.Valor, $"%{name}%")))
            .OrderBy(e => e.Name)
            .Skip((page - 1) * 10)
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    public async Task<AnimalProntuarioResponseDto?> GetProntuarioAsync(Guid animalId, CancellationToken ct)
    {
        var animal = await context.Animals
            .AsNoTracking()
            .Include(a => a.Raca)
                .ThenInclude(r => r!.Especie)
            .Include(a => a.Identificadores)
            .FirstOrDefaultAsync(a => a.Id == animalId, ct);

        if (animal is null)
            return null;

        var aplicacoes = await context.AplicacoesVacina
            .AsNoTracking()
            .Include(av => av.Vacina)
            .Where(av => av.AnimalId == animalId)
            .OrderByDescending(av => av.DataAplicacao)
            .ToListAsync(ct);

        return new AnimalProntuarioResponseDto
        {
            Animal = AnimalMapper.MapToResponse(animal),
            AplicacoesVacina = aplicacoes.Select(AplicacaoVacinaMapper.MapToResponse).ToList()
        };
    }
}