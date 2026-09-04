using Microsoft.AspNetCore.Mvc;
using Api.DTOs.Animals;
using Api.DTOs;
using Api.Mappers;
using Api.Services.Animals;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/animais")]
public class AnimalController(IAnimalService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AnimalResponseDto>>> GetAll([FromQuery] AnimalSearchDto search,
        CancellationToken cancellationToken)
    {
        var animals = await service.GetAllAsync(search, cancellationToken);
        var responses = animals.Select(AnimalMapper.MapToResponse).ToList();
        return Ok(responses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnimalResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var animal = await service.GetByIdAsync(id, cancellationToken);
        if (animal is null)
            return NotFound();

        var response = AnimalMapper.MapToResponse(animal);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<AnimalResponseDto>> Create(AnimalCreateDto dto, CancellationToken cancellationToken)
    {
        var animal = await service.CreateAsync(dto, cancellationToken);
        var fullAnimal = await service.GetByIdAsync(animal.Id, cancellationToken);
        if (fullAnimal is null)
            return BadRequest();

        var response = AnimalMapper.MapToResponse(fullAnimal);
        return CreatedAtAction(nameof(GetById), new { id = animal.Id }, response);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<AnimalResponseDto>> Patch(Guid id, AnimalPatchDto dto,
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

