using Api.Database;
using Api.DTOs.CartoesVacina;
using Api.DTOs;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.CartoesVacina;

public sealed class CartaoVacinaService(ApiContext context) : ICartaoVacinaService
{
    public Task<List<CartaoVacina>> GetAllAsync(CartaoVacinaSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.CartoesVacina
            .AsNoTracking();

        if (search?.VacinaId is not null)
            query = query.Where(cartao =>
                cartao.VacinasAplicadas.Any(aplicacao => aplicacao.VacinaId == search.VacinaId));
        if (search?.DataAplicacaoFrom is not null)
            query = query.Where(cartao =>
                cartao.VacinasAplicadas.Any(aplicacao => aplicacao.DataAplicacao >= search.DataAplicacaoFrom));
        if (search?.DataAplicacaoTo is not null)
            query = query.Where(cartao =>
                cartao.VacinasAplicadas.Any(aplicacao => aplicacao.DataAplicacao <= search.DataAplicacaoTo));

        return query.OrderBy(cartao => cartao.Id).ToListAsync(cancellationToken);
    }

    public Task<CartaoVacina?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.CartoesVacina
            .AsNoTracking()
            .Include(cartao => cartao.VacinasAplicadas)
            .ThenInclude(aplicacao => aplicacao.Vacina)
            .FirstOrDefaultAsync(cartao => cartao.Id == id, cancellationToken);
    }

    public async Task<CartaoVacina> CreateAsync(CartaoVacinaCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var cartao = new CartaoVacina
        {
            Id = Guid.NewGuid(),
            VacinasAplicadas = []
        };

        context.CartoesVacina.Add(cartao);

        if (dto.VacinasAplicadasIds is not null)
        {
            await ApplyApplicationsAsync(cartao.Id, [.. dto.VacinasAplicadasIds], cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        return cartao;
    }

    public async Task<CartaoVacina?> PatchAsync(Guid id, CartaoVacinaPatchDto dto,
        CancellationToken cancellationToken = default)
    {
        var cartao = await context.CartoesVacina.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (cartao is null)
        {
            return null;
        }

        if (dto.VacinasAplicadasIds is not null)
        {
            await ApplyApplicationsAsync(cartao.Id, [.. dto.VacinasAplicadasIds], cancellationToken,
                replaceExisting: true);
        }

        await context.SaveChangesAsync(cancellationToken);

        return cartao;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cartao = await context.CartoesVacina.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (cartao is null)
        {
            return false;
        }

        var animais = await context.Animals
            .Where(animal => animal.CartaoVacinaId == id)
            .ToListAsync(cancellationToken);

        foreach (var animal in animais)
        {
            animal.CartaoVacinaId = null;
        }

        var aplicacoes = await context.AplicacoesVacina
            .Where(aplicacao => aplicacao.CartaoVacinaId == id)
            .ToListAsync(cancellationToken);

        foreach (var aplicacao in aplicacoes)
        {
            aplicacao.CartaoVacinaId = null;
        }

        context.CartoesVacina.Remove(cartao);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ApplyApplicationsAsync(Guid cartaoId, HashSet<Guid>? applicationIds,
        CancellationToken cancellationToken, bool replaceExisting = false)
    {
        if (applicationIds is null)
        {
            return;
        }

        if (applicationIds.Any(id => id == Guid.Empty))
        {
            throw new ArgumentException("Application ids must not be empty.");
        }

        var currentApplications = await context.AplicacoesVacina
            .Where(aplicacao => aplicacao.CartaoVacinaId == cartaoId)
            .ToListAsync(cancellationToken);

        var targetApplications = await context.AplicacoesVacina
            .Where(aplicacao => applicationIds.Contains(aplicacao.Id))
            .ToListAsync(cancellationToken);

        if (targetApplications.Count != applicationIds.Count)
        {
            throw new KeyNotFoundException("One or more AplicacaoVacina records were not found.");
        }

        if (replaceExisting)
        {
            foreach (var application in currentApplications.Where(aplicacao => !applicationIds.Contains(aplicacao.Id)))
            {
                application.CartaoVacinaId = null;
            }
        }

        foreach (var application in targetApplications)
        {
            application.CartaoVacinaId = cartaoId;
        }
    }
}