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
    public async Task<ActionResult<List<VacinaReadResponseDto>>> GetAll(
        [FromQuery] VacinaSearchDto search,
        [FromQuery] int? page,
        CancellationToken cancellationToken)
    {
        var vacinas = await vacinaService.GetAllAsync(page, search, cancellationToken);
        var responses = vacinas.Select(VacinaMapper.MapToRead).ToList();
        return Ok(responses);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<VacinaShortResponseDto>>> GetAllByName(
        [FromQuery] int page = 1,
        [FromQuery] string? search = null,
        [FromQuery] string? name = null,
        [FromQuery] VacinaSearchDto? searchDto = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return Problem(detail: "page must be an positive number", statusCode: 400, title: "Error On Fetch Data");
        }

        var searchTerm = search ?? name ?? "";
        var vacinas = await vacinaService.GetAllByNameAsync(page, searchTerm, searchDto, cancellationToken);

        return vacinas.Select(VacinaMapper.MapToShort).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VacinaDetailResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var vacina = await vacinaService.GetByIdAsync(id, cancellationToken);
        if (vacina is null)
            return NotFound();

        var response = VacinaMapper.MapToResponse(vacina);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<VacinaDetailResponseDto>> Create(VacinaCreateDto dto,
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
    public async Task<ActionResult<VacinaDetailResponseDto>> Patch(Guid id, VacinaPatchDto dto,
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

    [HttpGet("count")]
    public async Task<ActionResult<int>> Count([FromQuery] VacinaSearchDto search, CancellationToken cancellationToken)
    {
        return Ok(await vacinaService.Count(search, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await vacinaService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}