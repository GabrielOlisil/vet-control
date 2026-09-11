using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using Api.Mappers;
using Api.Services.Animals;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/animais")]
public class AnimalController(IAnimalService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AnimalReadResponseDto>>> GetAll(
        [FromQuery] AnimalSearchDto search, [FromQuery] int? page,
        CancellationToken cancellationToken)
    {
        var animals = await service.GetAllAsync(page, search, cancellationToken);
        var responses = animals.Select(AnimalMapper.MapToHead).ToList();
        return Ok(responses);
    }


    [HttpGet("search")]
    public async Task<ActionResult<List<AnimalShortResponseDto>>> GetAllByName(CancellationToken cancellationToken, [FromQuery] int page = 1,
     [FromQuery] string name = "")
    {
        if (page < 1)
        {
            return Problem(detail: "page must be an positive number", statusCode: 400, title: "Error On Fetch Data");
        }

        return (await service.GetAllByNameAsync(page, name, cancellationToken)).Select(AnimalMapper.MapToShort).ToList();
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> Count([FromQuery] AnimalSearchDto search, CancellationToken cancellationToken)
    {
        return Ok(await service.Count(search, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnimalDetailResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var animal = await service.GetByIdAsync(id, cancellationToken);
        if (animal is null)
            return NotFound();

        var response = AnimalMapper.MapToResponse(animal);
        return Ok(response);
    }

    [HttpGet("{id:guid}/prontuario")]
    public async Task<ActionResult<AnimalProntuarioResponseDto>> GetProntuario(Guid id, CancellationToken ct)
    {
        var prontuario = await service.GetProntuarioAsync(id, ct);
        if (prontuario is null)
            return NotFound();

        return Ok(prontuario);
    }

    [HttpPost]

    public async Task<ActionResult<AnimalDetailResponseDto>> Create(AnimalCreateDto dto, CancellationToken cancellationToken)
    {
        var animal = await service.CreateAsync(dto, cancellationToken);
        var fullAnimal = await service.GetByIdAsync(animal.Id, cancellationToken);
        if (fullAnimal is null)
            return BadRequest();

        var response = AnimalMapper.MapToResponse(fullAnimal);
        return CreatedAtAction(nameof(GetById), new { id = animal.Id }, response);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<AnimalDetailResponseDto>> Patch(Guid id, AnimalPatchDto dto,
        CancellationToken cancellationToken)
    {
        var animal = await service.PatchAsync(id, dto, cancellationToken);
        if (animal is null)
            return NotFound();

        var fullAnimal = await service.GetByIdAsync(id, cancellationToken);
        if (fullAnimal is null)
            return BadRequest();

        var response = AnimalMapper.MapToResponse(fullAnimal);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
