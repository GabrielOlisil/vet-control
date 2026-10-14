using Api.DTOs;
using Api.Mappers;
using Api.Security;
using Api.Services.ProtocolosVacinais;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/protocolos-vacinais")]
public class ProtocoloVacinalController(IProtocoloVacinalService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProtocoloVacinalReadResponseDto>>> GetAll(
        [FromQuery] ProtocoloVacinalSearchDto search, [FromQuery] int? page,
        CancellationToken cancellationToken)
    {
        var items = await service.GetAllAsync(page, search, cancellationToken);
        return Ok(items.Select(ProtocoloVacinalMapper.MapToRead).ToList());
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> Count([FromQuery] ProtocoloVacinalSearchDto search,
        CancellationToken cancellationToken)
    {
        return Ok(await service.CountAsync(search, cancellationToken));
    }

    [HttpGet("pendencias")]
    public async Task<ActionResult<List<ProtocoloVacinalPendenciaResponseDto>>> GetPendencias(
        [FromQuery] ProtocoloVacinalPendenciaSearchDto search, [FromQuery] int? page,
        CancellationToken cancellationToken)
    {
        return Ok(await service.GetPendenciasAsync(page, search, cancellationToken));
    }

    [HttpGet("pendencias/count")]
    public async Task<ActionResult<int>> CountPendencias(
        [FromQuery] ProtocoloVacinalPendenciaSearchDto search, CancellationToken cancellationToken)
    {
        return Ok(await service.CountPendenciasAsync(search, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProtocoloVacinalDetailResponseDto>> GetById(Guid id,
        CancellationToken cancellationToken)
    {
        var item = await service.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(ProtocoloVacinalMapper.MapToDetail(item));
    }

    [HttpPost]
    [Authorize(Policy = ClinicalPolicies.ManageProtocols)]
    public async Task<ActionResult<ProtocoloVacinalDetailResponseDto>> Create(
        ProtocoloVacinalCreateDto dto, CancellationToken cancellationToken)
    {
        var item = await service.CreateAsync(dto, cancellationToken);
        var fullItem = await service.GetByIdAsync(item.Id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ProtocoloVacinalMapper.MapToDetail(fullItem!));
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = ClinicalPolicies.ManageProtocols)]
    public async Task<ActionResult<ProtocoloVacinalDetailResponseDto>> Patch(Guid id,
        ProtocoloVacinalPatchDto dto, CancellationToken cancellationToken)
    {
        var item = await service.PatchAsync(id, dto, cancellationToken);
        if (item is null)
            return NotFound();

        var fullItem = await service.GetByIdAsync(id, cancellationToken);
        return Ok(ProtocoloVacinalMapper.MapToDetail(fullItem!));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = ClinicalPolicies.ManageProtocols)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpGet("~/api/v1/animais/{animalId:guid}/protocolos-vacinais")]
    public async Task<ActionResult<List<AnimalProtocoloVacinalReadResponseDto>>> GetAnimalProtocols(Guid animalId,
        CancellationToken cancellationToken)
    {
        var items = await service.GetAnimalProtocolsAsync(animalId, cancellationToken);
        return Ok(items.Select(ProtocoloVacinalMapper.MapAnimalProtocol).ToList());
    }

    [HttpPost("~/api/v1/animais/{animalId:guid}/protocolos-vacinais")]
    [Authorize(Policy = ClinicalPolicies.ManageProtocols)]
    public async Task<ActionResult<AnimalProtocoloVacinalReadResponseDto>> AssignToAnimal(Guid animalId,
        AnimalProtocoloVacinalCreateDto dto, CancellationToken cancellationToken)
    {
        var item = await service.AssignToAnimalAsync(animalId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetAnimalProtocolById), new { animalId, id = item.Id },
            ProtocoloVacinalMapper.MapAnimalProtocol(item));
    }

    [HttpGet("~/api/v1/animais/{animalId:guid}/protocolos-vacinais/{id:guid}")]
    public async Task<ActionResult<AnimalProtocoloVacinalReadResponseDto>> GetAnimalProtocolById(Guid animalId,
        Guid id, CancellationToken cancellationToken)
    {
        var item = await service.GetAnimalProtocolByIdAsync(id, cancellationToken);
        if (item is null || item.AnimalId != animalId)
            return NotFound();

        return Ok(ProtocoloVacinalMapper.MapAnimalProtocol(item));
    }

    [HttpPatch("~/api/v1/animais/{animalId:guid}/protocolos-vacinais/{id:guid}")]
    [Authorize(Policy = ClinicalPolicies.ManageProtocols)]
    public async Task<ActionResult<AnimalProtocoloVacinalReadResponseDto>> PatchAnimalProtocol(Guid animalId,
        Guid id, AnimalProtocoloVacinalPatchDto dto, CancellationToken cancellationToken)
    {
        var existing = await service.GetAnimalProtocolByIdAsync(id, cancellationToken);
        if (existing is null || existing.AnimalId != animalId)
            return NotFound();

        var item = await service.PatchAnimalProtocolAsync(id, dto, cancellationToken);
        return Ok(ProtocoloVacinalMapper.MapAnimalProtocol(item!));
    }
}
