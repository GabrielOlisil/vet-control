using Api.DTOs;
using Api.Mappers;
using Api.Security;
using Api.Services.MovimentacoesAnimais;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/movimentacoes-animais")]
public class MovimentacaoAnimalController(IMovimentacaoAnimalService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MovimentacaoAnimalResponseDto>>> GetAll(
        [FromQuery] MovimentacaoAnimalSearchDto search, [FromQuery] int? page,
        CancellationToken cancellationToken)
    {
        var items = await service.GetAllAsync(page, search, cancellationToken);
        return Ok(items.Select(MovimentacaoAnimalMapper.MapToResponse).ToList());
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> Count([FromQuery] MovimentacaoAnimalSearchDto search,
        CancellationToken cancellationToken)
    {
        return Ok(await service.CountAsync(search, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MovimentacaoAnimalResponseDto>> GetById(Guid id,
        CancellationToken cancellationToken)
    {
        var item = await service.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(MovimentacaoAnimalMapper.MapToResponse(item));
    }

    [HttpPost("lote")]
    [Authorize(Policy = ClinicalPolicies.ManageMovements)]
    public async Task<ActionResult<List<MovimentacaoAnimalResponseDto>>> CreateBatch(
        MovimentacaoAnimalLoteCreateDto dto, CancellationToken cancellationToken)
    {
        var items = await service.CreateManyAsync(dto, cancellationToken);
        return Ok(items.Select(MovimentacaoAnimalMapper.MapToResponse).ToList());
    }

    [HttpGet("~/api/v1/animais/{animalId:guid}/movimentacoes")]
    public async Task<ActionResult<List<MovimentacaoAnimalResponseDto>>> GetByAnimal(Guid animalId,
        [FromQuery] int? page, CancellationToken cancellationToken)
    {
        var items = await service.GetAllAsync(page, new MovimentacaoAnimalSearchDto { AnimalId = animalId }, cancellationToken);
        return Ok(items.Select(MovimentacaoAnimalMapper.MapToResponse).ToList());
    }

    [HttpPost("~/api/v1/animais/{animalId:guid}/movimentacoes")]
    [Authorize(Policy = ClinicalPolicies.ManageMovements)]
    public async Task<ActionResult<MovimentacaoAnimalResponseDto>> Create(Guid animalId,
        MovimentacaoAnimalCreateDto dto, CancellationToken cancellationToken)
    {
        var item = await service.CreateAsync(animalId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MovimentacaoAnimalMapper.MapToResponse(item));
    }
}
