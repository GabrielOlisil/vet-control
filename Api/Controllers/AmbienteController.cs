using Api.Database;
using Api.DTOs;
using Api.Models;
using Api.Security;
using Api.Services;
using Api.Services.Ambientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/ambientes")]
public sealed class AmbienteController(ApiContext context, AmbienteService service) : ControllerBase
{
    private const int PageSize = 10;

    [HttpGet]
    public Task<List<AmbienteResponseDto>> GetAll([FromQuery] int? page, [FromQuery] string? search, CancellationToken ct) =>
        service.Search(search).OrderBy(a => a.NomeNormalizado).ThenBy(a => a.Id)
            .Skip(Pagination.Offset(page, PageSize)).Take(PageSize)
            .Select(a => new AmbienteResponseDto(a.Id, a.Nome, a.Tipo)).ToListAsync(ct);

    [HttpGet("count")]
    public Task<int> Count([FromQuery] string? search, CancellationToken ct) => service.Search(search).CountAsync(ct);

    [HttpGet("ocupacao")]
    public Task<List<AmbienteOcupacaoDto>> Occupancy(CancellationToken ct) =>
        context.Ambientes.AsNoTracking().OrderBy(a => a.NomeNormalizado).ThenBy(a => a.Id)
            .Select(a => new AmbienteOcupacaoDto(a.Id, a.Nome, a.Tipo, a.Animais.Count)).ToListAsync(ct);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AmbienteResponseDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await context.Ambientes.AsNoTracking().Where(a => a.Id == id)
            .Select(a => new AmbienteResponseDto(a.Id, a.Nome, a.Tipo)).SingleOrDefaultAsync(ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = ClinicalPolicies.ManageMovements)]
    public async Task<ActionResult<AmbienteResponseDto>> Create(AmbienteCreateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome) || !Enum.IsDefined(dto.Tipo))
            throw new ArgumentException("Informe um nome e um tipo de ambiente válido.");
        var ambiente = new Ambiente { Id = Guid.NewGuid(), Nome = dto.Nome.Trim(), Tipo = dto.Tipo };
        context.Ambientes.Add(ambiente);
        await context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = ambiente.Id },
            new AmbienteResponseDto(ambiente.Id, ambiente.Nome, ambiente.Tipo));
    }
}
