using Microsoft.AspNetCore.Mvc;
using Api.DTOs.Especies;
using Api.DTOs;
using Api.Mappers;
using Api.Services.Especies;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/especies")]
public class EspecieController(IEspecieService especieService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<EspecieReadResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var especies = await especieService.GetAllAsync(cancellationToken);
        var responses = especies.Select(EspecieMapper.MapToHead).ToList();
        return Ok(responses);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> Count(CancellationToken cancellationToken)
    {
        return Ok(await especieService.Count(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EspecieDetailResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var especie = await especieService.GetByIdAsync(id, cancellationToken);
        if (especie is null)
            return NotFound();

        var response = EspecieMapper.MapToResponse(especie);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<EspecieDetailResponseDto>> Create(EspecieCreateDto dto,
        CancellationToken cancellationToken)
    {
        var especie = await especieService.CreateAsync(dto, cancellationToken);
        var fullEspecie = await especieService.GetByIdAsync(especie.Id, cancellationToken);
        if (fullEspecie is null)
            return BadRequest();

        var response = EspecieMapper.MapToResponse(fullEspecie);
        return CreatedAtAction(nameof(GetById), new { id = especie.Id }, response);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<EspecieDetailResponseDto>> Patch(Guid id, EspeciePatchDto dto,
        CancellationToken cancellationToken)
    {
        var especie = await especieService.PatchAsync(id, dto, cancellationToken);
        if (especie is null)
            return NotFound();

        var fullEspecie = await especieService.GetByIdAsync(id, cancellationToken);
        if (fullEspecie is null)
            return BadRequest();

        var response = EspecieMapper.MapToResponse(fullEspecie);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await especieService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
