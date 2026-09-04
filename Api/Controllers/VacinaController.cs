using Api.DTOs.Vacinas;
using Api.DTOs;
using Api.Mappers;
using Api.Services.Vacinas;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/vacinas")]
public class VacinaController(IVacinaService vacinaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<VacinaResponseDto>>> GetAll([FromQuery] VacinaSearchDto search,
        CancellationToken cancellationToken)
    {
        var vacinas = await vacinaService.GetAllAsync(search, cancellationToken);
        var responses = vacinas.Select(VacinaMapper.MapToResponse).ToList();
        return Ok(responses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VacinaResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var vacina = await vacinaService.GetByIdAsync(id, cancellationToken);
        if (vacina is null)
            return NotFound();

        var response = VacinaMapper.MapToResponse(vacina);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<VacinaResponseDto>> Create(VacinaCreateDto dto,
        CancellationToken cancellationToken)
    {
        var vacina = await vacinaService.CreateAsync(dto, cancellationToken);
        var fullVacina = await vacinaService.GetByIdAsync(vacina.Id, cancellationToken);
        if (fullVacina is null)
            return BadRequest();

        var response = VacinaMapper.MapToResponse(fullVacina);
        return CreatedAtAction(nameof(GetById), new { id = vacina.Id }, response);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<VacinaResponseDto>> Patch(Guid id, VacinaPatchDto dto,
        CancellationToken cancellationToken)
    {
        var vacina = await vacinaService.PatchAsync(id, dto, cancellationToken);
        if (vacina is null)
            return NotFound();

        var fullVacina = await vacinaService.GetByIdAsync(id, cancellationToken);
        if (fullVacina is null)
            return BadRequest();

        var response = VacinaMapper.MapToResponse(fullVacina);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await vacinaService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}