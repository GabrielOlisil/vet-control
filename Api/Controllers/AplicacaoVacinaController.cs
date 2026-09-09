using Api.DTOs.AplicacoesVacina;
using Api.DTOs;
using Api.Mappers;
using Api.Services.AplicacoesVacina;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/aplicacoes-vacina")]
public class AplicacaoVacinaController(IAplicacaoVacinaService aplicacaoVacinaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AplicacaoVacinaReadResponseDto>>> GetAll([FromQuery] int? page,
        [FromQuery] AplicacaoVacinaSearchDto search, CancellationToken cancellationToken)
    {
        var aplicacoes = await aplicacaoVacinaService.GetAllAsync(page, search, cancellationToken);




        var responses = aplicacoes.Select(AplicacaoVacinaMapper.MapToHead).ToList();
        return Ok(responses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AplicacaoVacinaDetailResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var aplicacao = await aplicacaoVacinaService.GetByIdAsync(id, cancellationToken);
        if (aplicacao is null)
            return NotFound();

        var response = AplicacaoVacinaMapper.MapToResponse(aplicacao);
        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> Count([FromQuery] AplicacaoVacinaSearchDto? search = null, CancellationToken cancellationToken = default)
    {
        return Ok(await aplicacaoVacinaService.Count(search, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<AplicacaoVacinaDetailResponseDto>> Create(AplicacaoVacinaCreateDto dto,
        CancellationToken cancellationToken)
    {
        var aplicacao = await aplicacaoVacinaService.CreateAsync(dto, cancellationToken);
        var fullAplicacao = await aplicacaoVacinaService.GetByIdAsync(aplicacao.Id, cancellationToken);
        if (fullAplicacao is null)
            return BadRequest();

        var response = AplicacaoVacinaMapper.MapToResponse(fullAplicacao);
        return CreatedAtAction(nameof(GetById), new { id = aplicacao.Id }, response);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<AplicacaoVacinaDetailResponseDto>> Patch(Guid id, AplicacaoVacinaPatchDto dto,
        CancellationToken cancellationToken)
    {
        var aplicacao = await aplicacaoVacinaService.PatchAsync(id, dto, cancellationToken);
        if (aplicacao is null)
            return NotFound();

        var fullAplicacao = await aplicacaoVacinaService.GetByIdAsync(id, cancellationToken);
        if (fullAplicacao is null)
            return BadRequest();

        var response = AplicacaoVacinaMapper.MapToResponse(fullAplicacao);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await aplicacaoVacinaService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}