using Api.Database;
using Api.DTOs.Animals;
using Api.DTOs;
using Api.Models;
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
            .Include(a => a.CartaoVacina)
            .ThenInclude(c => c!.VacinasAplicadas)
            .ThenInclude(av => av.Vacina)
            .FirstOrDefaultAsync(animal => animal.Id == id, cancellationToken);
    }

    public Task<List<Animal>> GetAllAsync(int? page, AnimalSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Animals.Include(e => e.Raca)
            .AsNoTracking();

        if (search?.RacaId is not null)
            query = query.Where(animal => animal.RacaId == search.RacaId);
        if (search?.CartaoVacinaId is not null)
            query = query.Where(animal => animal.CartaoVacinaId == search.CartaoVacinaId);
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
        var animal = new Animal
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            RacaId = dto.RacaId,
            CartaoVacinaId = dto.CartaoVacinaId,
            DataNascimento = dto.DataNascimento,
            PictureUpload = dto.PictureUpload
        };

        context.Animals.Add(animal);
        await context.SaveChangesAsync(cancellationToken);

        return animal;
    }

    public async Task<Animal?> PatchAsync(Guid id, AnimalPatchDto dto,
        CancellationToken cancellationToken = default)
    {
        var animal = await context.Animals.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
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

        if (dto.PictureUpload is not null)
        {
            animal.PictureUpload = dto.PictureUpload;
        }

        if (dto.RacaId.HasValue)
        {
            animal.RacaId = dto.RacaId.Value;
        }

        if (dto.CartaoVacinaId.HasValue)
        {
            animal.CartaoVacinaId = dto.CartaoVacinaId;
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
        if (search?.CartaoVacinaId is not null)
            query = query.Where(animal => animal.CartaoVacinaId == search.CartaoVacinaId);
        if (search?.DataNascimentoFrom is not null)
            query = query.Where(animal => animal.DataNascimento >= search.DataNascimentoFrom);
        if (search?.DataNascimentoTo is not null)
            query = query.Where(animal => animal.DataNascimento <= search.DataNascimentoTo);

        return query.CountAsync(cancellationToken);
    }
}