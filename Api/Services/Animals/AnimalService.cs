using Api.Database;
using Api.DTOs;
using Api.Mappers;
using Api.Models;
using Api.Models.Enums;
using Api.Services;
using Api.Services.Ambientes;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Animals;

public sealed class AnimalService(ApiContext context, ClinicalCalendar calendar, AmbienteService ambientes) : IAnimalService
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
                .ThenInclude(r => r!.Especie)
            .Include(a => a.Identificadores)
            .AsNoTracking();

        query = ApplyFilters(query, search);

        query = query.OrderBy(animal => animal.Name).ThenBy(animal => animal.CreationDateTime)
                    .ThenBy(animal => animal.Id);

        if (page.HasValue)
        {
            query = query.Skip(Pagination.Offset(page, 10)).Take(10);
        }

        return query.ToListAsync(cancellationToken);
    }

    public async Task<Animal> CreateAsync(AnimalCreateDto dto, CancellationToken cancellationToken = default)
    {
        ValidateBirthDate(dto.DataNascimento);
        var principalIndex = ValidateIdentifiers(dto.Identificadores);
        var ambiente = await ambientes.ResolveAsync(dto.AmbienteId, dto.LoteOuPasto, cancellationToken);

        var animal = new Animal
        {
            Id = Guid.NewGuid(),
            Name = dto.Name ?? string.Empty,
            RacaId = dto.RacaId,
            DataNascimento = dto.DataNascimento,
            DataNascimentoAproximada = dto.DataNascimentoAproximada,
            Sexo = dto.Sexo,
            OrigemAnimal = dto.Origem,
            LoteOuPasto = ambiente?.Nome,
            AmbienteId = ambiente?.Id,
            Ativo = dto.Ativo,
            Identificadores = dto.Identificadores.Select((i, index) => new IdentificadorAnimal
            {
                Id = Guid.NewGuid(),
                Tipo = i.Tipo,
                Valor = i.Valor,
                IsPrincipal = index == principalIndex
            }).ToList()
        };

        if (ambiente is not null)
        {
            animal.Movimentacoes.Add(new MovimentacaoAnimal
            {
                Id = Guid.NewGuid(),
                DataMovimentacao = calendar.Today,
                Tipo = TipoMovimentacaoAnimal.Entrada,
                LocalDestino = ambiente.Nome,
                AmbienteDestinoId = ambiente.Id,
                Motivo = "Cadastro do animal"
            });
        }

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

        if (dto.NameWasProvided)
        {
            animal.Name = dto.Name;
        }

        if (dto.DataNascimento.HasValue)
        {
            ValidateBirthDate(dto.DataNascimento.Value);
            animal.DataNascimento = dto.DataNascimento.Value;
        }

        if (dto.DataNascimentoAproximada.HasValue)
        {
            animal.DataNascimentoAproximada = dto.DataNascimentoAproximada.Value;
        }

        if (dto.RacaIdWasProvided)
        {
            animal.RacaId = dto.RacaId;
        }

        if (dto.Sexo.HasValue)
        {
            animal.Sexo = dto.Sexo.Value;
        }

        if (dto.Origem.HasValue)
        {
            animal.OrigemAnimal = dto.Origem.Value;
        }

        if (dto.LoteOuPastoWasProvided || dto.AmbienteIdWasProvided)
        {
            var destino = await ambientes.ResolveAsync(dto.AmbienteId, dto.LoteOuPasto, cancellationToken);
            if (animal.AmbienteId != destino?.Id || (destino is null && animal.LoteOuPasto is not null))
            {
                if (!animal.Ativo && dto.Ativo != true)
                    throw new ArgumentException("Para corrigir a localização de um animal inativo, registre um ajuste com motivo no histórico de movimentação.");
                context.MovimentacoesAnimais.Add(new MovimentacaoAnimal
                {
                    Id = Guid.NewGuid(),
                    AnimalId = animal.Id,
                    DataMovimentacao = calendar.Today,
                    Tipo = destino is null ? TipoMovimentacaoAnimal.Saida
                        : string.IsNullOrWhiteSpace(animal.LoteOuPasto) ? TipoMovimentacaoAnimal.Entrada : TipoMovimentacaoAnimal.Transferencia,
                    LocalOrigem = animal.LoteOuPasto,
                    AmbienteOrigemId = animal.AmbienteId,
                    LocalDestino = destino?.Nome,
                    AmbienteDestinoId = destino?.Id,
                    Motivo = "Alteração de ambiente no cadastro"
                });
                animal.LoteOuPasto = destino?.Nome;
                animal.AmbienteId = destino?.Id;
            }
        }

        if (dto.Ativo.HasValue)
        {
            animal.Ativo = dto.Ativo.Value;
        }

        if (dto.Identificadores is not null)
            UpdateIdentifiers(animal, dto.Identificadores);

        await context.SaveChangesAsync(cancellationToken);

        return animal;
    }

    private void ValidateBirthDate(DateOnly date)
    {
        if (date == default || date > calendar.Today)
            throw new ArgumentException("Informe uma data de nascimento válida, até o dia de hoje na clínica.");
    }

    private void UpdateIdentifiers(Animal animal, List<IdentificadorCreateDto> identifiers)
    {
        var principalIndex = ValidateIdentifiers(identifiers);
        var removed = animal.Identificadores.ToDictionary(i => (i.Tipo, i.Valor));
        for (var index = 0; index < identifiers.Count; index++)
        {
            var input = identifiers[index];
            if (removed.Remove((input.Tipo, input.Valor), out var identifier))
            {
                identifier.IsPrincipal = index == principalIndex;
                continue;
            }

            context.IdentificadorAnimal.Add(new IdentificadorAnimal
            {
                Id = Guid.NewGuid(),
                AnimalId = animal.Id,
                Tipo = input.Tipo,
                Valor = input.Valor,
                IsPrincipal = index == principalIndex
            });
        }
        context.IdentificadorAnimal.RemoveRange(removed.Values);
    }

    private static int ValidateIdentifiers(List<IdentificadorCreateDto> identifiers)
    {
        if (identifiers.Count == 0)
            throw new ArgumentException("Ao menos um identificador é obrigatório.");
        if (identifiers.Any(i => string.IsNullOrWhiteSpace(i.Valor)))
            throw new ArgumentException("Informe o valor de cada identificador.");
        if (identifiers.Count(i => i.EhPrincipal) > 1)
            throw new ArgumentException("Selecione somente um identificador principal.");
        return Math.Max(0, identifiers.FindIndex(i => i.EhPrincipal));
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var animal = await context.Animals.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (animal is null)
        {
            return false;
        }

        if (await context.AplicacoesVacina.AnyAsync(a => a.AnimalId == id, cancellationToken) ||
            await context.MovimentacoesAnimais.AnyAsync(m => m.AnimalId == id, cancellationToken) ||
            await context.AnimalProtocolosVacinais.AnyAsync(p => p.AnimalId == id, cancellationToken))
            throw new InvalidOperationException("Este animal possui histórico. Inative o cadastro para preservá-lo.");

        context.Animals.Remove(animal);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<int> Count(AnimalSearchDto? search = null, CancellationToken cancellationToken = default)
    {
        var query = context.Animals.AsNoTracking();
        query = ApplyFilters(query, search);
        return query.CountAsync(cancellationToken);
    }

    private static IQueryable<Animal> ApplyFilters(IQueryable<Animal> query, AnimalSearchDto? search)
    {
        if (search is null)
            return query;

        if (search.RacaId.HasValue)
            query = query.Where(animal => animal.RacaId == search.RacaId.Value);

        if (search.EspecieId.HasValue)
            query = query.Where(animal => animal.Raca != null && animal.Raca.EspecieId == search.EspecieId.Value);

        if (search.Sexo.HasValue)
            query = query.Where(animal => animal.Sexo == search.Sexo.Value);

        var origem = search.Origem ?? search.OrigemAnimal;
        if (origem.HasValue)
            query = query.Where(animal => animal.OrigemAnimal == origem.Value);

        if (search.Ativo.HasValue)
            query = query.Where(animal => animal.Ativo == search.Ativo.Value);

        if (!string.IsNullOrWhiteSpace(search.LoteOuPasto))
            query = query.Where(animal => animal.LoteOuPasto == search.LoteOuPasto);
        if (search.AmbienteId.HasValue)
            query = query.Where(animal => animal.AmbienteId == search.AmbienteId);

        if (search.DataNascimento.HasValue)
            query = query.Where(animal => animal.DataNascimento == search.DataNascimento.Value);

        if (search.DataNascimentoFrom.HasValue)
            query = query.Where(animal => animal.DataNascimento >= search.DataNascimentoFrom.Value);

        if (search.DataNascimentoTo.HasValue)
            query = query.Where(animal => animal.DataNascimento <= search.DataNascimentoTo.Value);

        if (!string.IsNullOrWhiteSpace(search.Identificador))
            query = query.Where(animal => animal.Identificadores.Any(i => EF.Functions.ILike(i.Valor, $"%{search.Identificador}%")));

        if (search.TipoIdentificador.HasValue)
            query = query.Where(animal => animal.Identificadores.Any(i => i.Tipo == search.TipoIdentificador.Value));

        if (search.CreationDateTimeFrom.HasValue)
            query = query.Where(animal => animal.CreationDateTime >= search.CreationDateTimeFrom.Value);

        if (search.CreationDateTimeTo.HasValue)
            query = query.Where(animal => animal.CreationDateTime <= search.CreationDateTimeTo.Value);

        return query;
    }


    public Task<List<Animal>> GetAllByNameAsync(int page, string name, AnimalSearchDto? search = null, CancellationToken cancellationToken = default)
    {
        var query = context.Animals
            .Include(a => a.Raca)
                .ThenInclude(r => r!.Especie)
            .Include(a => a.Identificadores)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(e => EF.Functions.ILike(e.Name ?? string.Empty, $"%{name}%")
                || e.Identificadores.Any(i => EF.Functions.ILike(i.Valor, $"%{name}%")));
        }

        query = ApplyFilters(query, search);

        var currentPage = Pagination.NormalizePage(page);
        return query
            .OrderBy(e => e.Name)
            .Skip(Pagination.Offset(currentPage, 10))
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
            .Include(av => av.SubstituidaPor)
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
