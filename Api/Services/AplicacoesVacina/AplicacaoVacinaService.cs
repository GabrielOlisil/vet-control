using Api.Database;
using Api.DTOs;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.AplicacoesVacina;

public sealed class AplicacaoVacinaService(ApiContext context, IWebHostEnvironment env) : IAplicacaoVacinaService
{
    public async Task<AplicacaoVacina> CreateAsync(AplicacaoVacinaCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var animal = await context.Animals.FindAsync([dto.AnimalId], cancellationToken);
        if (animal is null)
            throw new KeyNotFoundException($"Animal com Id '{dto.AnimalId}' não encontrado.");

        var dataProximaDose = dto.DataProximaDose;
        if (dataProximaDose is null)
        {
            var vacina = await context.Vacinas.FindAsync([dto.VacinaId], cancellationToken);
            if (vacina is not null && vacina.ReaplicarEmXDias > 0)
            {
                dataProximaDose = dto.DataAplicacao.AddDays((int)vacina.ReaplicarEmXDias);
            }
            else
            {
                dataProximaDose = dto.DataAplicacao;
            }
        }

        var aplicacao = new AplicacaoVacina
        {
            Id = Guid.NewGuid(),
            AnimalId = dto.AnimalId,
            VacinaId = dto.VacinaId,
            DataAplicacao = dto.DataAplicacao,
            DataProximaDose = dataProximaDose.Value,
            NumeroLote = dto.NumeroLote,
            DoseMl = dto.DoseMl,
            Observacoes = dto.Observacoes
        };

        context.AplicacoesVacina.Add(aplicacao);
        await context.SaveChangesAsync(cancellationToken);

        return aplicacao;
    }

    public async Task<AplicacaoVacina?> PatchAsync(Guid id, AplicacaoVacinaPatchDto dto,
        CancellationToken cancellationToken = default)
    {
        var aplicacao =
            await context.AplicacoesVacina.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (aplicacao is null)
        {
            return null;
        }

        if (dto.AnimalId.HasValue)
        {
            aplicacao.AnimalId = dto.AnimalId.Value;
        }

        if (dto.VacinaId.HasValue)
        {
            aplicacao.VacinaId = dto.VacinaId.Value;
        }

        if (dto.DataAplicacao.HasValue)
        {
            aplicacao.DataAplicacao = dto.DataAplicacao.Value;
        }

        if (dto.DataProximaDose.HasValue)
        {
            aplicacao.DataProximaDose = dto.DataProximaDose.Value;
        }

        if (dto.NumeroLote is not null)
        {
            aplicacao.NumeroLote = dto.NumeroLote;
        }

        if (dto.DoseMl is not null)
        {
            aplicacao.DoseMl = dto.DoseMl;
        }

        if (dto.Observacoes is not null)
        {
            aplicacao.Observacoes = dto.Observacoes;
        }

        await context.SaveChangesAsync(cancellationToken);

        return aplicacao;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var aplicacao =
            await context.AplicacoesVacina.FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        if (aplicacao is null)
        {
            return false;
        }

        context.AplicacoesVacina.Remove(aplicacao);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<AplicacaoVacina?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.AplicacoesVacina
            .AsNoTracking()
            .Include(aplicacao => aplicacao.Vacina)
                .ThenInclude(v => v!.Especie)
            .Include(aplicacao => aplicacao.Animal)
                .ThenInclude(a => a!.Identificadores)
            .Include(aplicacao => aplicacao.Animal)
                .ThenInclude(a => a!.Raca)
            .FirstOrDefaultAsync(aplicacao => aplicacao.Id == id, cancellationToken);
    }

    public Task<List<AplicacaoVacina>> GetAllAsync(int? page, AplicacaoVacinaSearchDto? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.AplicacoesVacina
            .Include(aplicacao => aplicacao.Vacina)
            .Include(aplicacao => aplicacao.Animal)
                .ThenInclude(a => a!.Identificadores)
            .AsNoTracking();


        if (search?.VacinaId is not null)
            query = query.Where(aplicacao => aplicacao.VacinaId == search.VacinaId);
        if (search?.AnimalId is not null)
            query = query.Where(aplicacao => aplicacao.AnimalId == search.AnimalId);
        if (search?.DataAplicacaoFrom is not null)
            query = query.Where(aplicacao => aplicacao.DataAplicacao >= search.DataAplicacaoFrom);
        if (search?.DataAplicacaoTo is not null)
            query = query.Where(aplicacao => aplicacao.DataAplicacao <= search.DataAplicacaoTo);

        if (!page.HasValue)
        {
            page = 1;
        }
        query = query.OrderByDescending(aplicacao => aplicacao.DataAplicacao)
            .Skip((page.Value - 1) * 10).Take(10);

        return query.ToListAsync(cancellationToken);
    }

    public Task<int> Count(AplicacaoVacinaSearchDto? search = null, CancellationToken cancellationToken = default)
    {
        var query = context.AplicacoesVacina
                   .AsNoTracking();

        if (search?.VacinaId is not null)
            query = query.Where(aplicacao => aplicacao.VacinaId == search.VacinaId);
        if (search?.AnimalId is not null)
            query = query.Where(aplicacao => aplicacao.AnimalId == search.AnimalId);
        if (search?.DataAplicacaoFrom is not null)
            query = query.Where(aplicacao => aplicacao.DataAplicacao >= search.DataAplicacaoFrom);
        if (search?.DataAplicacaoTo is not null)
            query = query.Where(aplicacao => aplicacao.DataAplicacao <= search.DataAplicacaoTo);

        return query.CountAsync(cancellationToken);
    }

    public async Task<bool> UploadComprovanteAsync(Guid aplicacaoId, Stream streamArquivo, string contentType, CancellationToken ct)
    {
        var aplicacao = await context.AplicacoesVacina.FirstOrDefaultAsync(e => e.Id == aplicacaoId, ct);
        if (aplicacao is null)
            return false;

        var dir = Path.Combine(env.WebRootPath, "comprovantes");
        Directory.CreateDirectory(dir);

        var filePath = Path.Combine(dir, $"{aplicacaoId}.pdf");
        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await streamArquivo.CopyToAsync(fs, ct);

        await context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<(byte[] Bytes, string ContentType)?> GetComprovanteAsync(Guid aplicacaoId, CancellationToken ct)
    {
        var aplicacao = await context.AplicacoesVacina
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == aplicacaoId, ct);
        if (aplicacao is null)
            return null;

        var filePath = Path.Combine(env.WebRootPath, "comprovantes", $"{aplicacaoId}.pdf");
        if (!File.Exists(filePath))
            return null;

        var bytes = await File.ReadAllBytesAsync(filePath, ct);
        return (bytes, "application/pdf");
    }
}