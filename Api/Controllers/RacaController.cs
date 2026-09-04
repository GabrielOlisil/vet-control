using Microsoft.AspNetCore.Mvc;
using Api.DTOs.Racas;
using Api.DTOs;
using Api.Mappers;
using Api.Services.Racas;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/racas")]
public class RacaController(IRacaService racaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RacaResponseDto>>> GetAll([FromQuery] RacaSearchDto search,
        CancellationToken cancellationToken)
    {
        var racas = await racaService.GetAllAsync(search, cancellationToken);
        var responses = racas.Select(RacaMapper.MapToResponse).ToList();
        return Ok(responses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RacaResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var raca = await racaService.GetByIdAsync(id, cancellationToken);
        if (raca is null)
            return NotFound();

        var response = RacaMapper.MapToResponse(raca);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<RacaResponseDto>> Create(RacaCreateDto dto, CancellationToken cancellationToken)
    {
        var raca = await racaService.CreateAsync(dto, cancellationToken);
        var fullRaca = await racaService.GetByIdAsync(raca.Id, cancellationToken);
        if (fullRaca is null)
            return BadRequest();

        var response = RacaMapper.MapToResponse(fullRaca);
        return CreatedAtAction(nameof(GetById), new { id = raca.Id }, response);
    }


    [HttpGet]
    [Route("Throw")]
    public IActionResult Throw()
    {
        throw new DbUpdateException("Voce ta vendo um erro");
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<RacaResponseDto>> Patch(Guid id, RacaPatchDto dto,
        CancellationToken cancellationToken)
    {
        var raca = await racaService.PatchAsync(id, dto, cancellationToken);
        if (raca is null)
            return NotFound();


        var fullRaca = await racaService.GetByIdAsync(id, cancellationToken);
        if (fullRaca is null)
            return BadRequest();

        var response = RacaMapper.MapToResponse(fullRaca);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await racaService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
