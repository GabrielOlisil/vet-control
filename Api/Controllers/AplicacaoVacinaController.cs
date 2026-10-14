using Api.DTOs;
using Api.Mappers;
using Api.Models.Enums;
using Api.Security;
using Api.Services.AplicacoesVacina;
using Microsoft.AspNetCore.Authorization;
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

    [HttpGet("{id:guid}/validar-assinatura")]
    public async Task<ActionResult<AssinaturaValidacaoResponseDto>> ValidarAssinatura(Guid id,
        CancellationToken cancellationToken)
    {
        var resultado = await aplicacaoVacinaService.ValidarAssinaturaAsync(id, cancellationToken);
        if (resultado is null)
            return NotFound();

        return Ok(resultado);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> Count(
        [FromQuery] AplicacaoVacinaSearchDto? search = null,
        [FromQuery] bool? somenteAtrasadas = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await aplicacaoVacinaService.Count(search, somenteAtrasadas, cancellationToken));
    }

    [HttpGet("atrasadas")]
    public async Task<ActionResult<List<AplicacaoVacinaReadResponseDto>>> GetAtrasadas(
        [FromQuery] int? page,
        [FromQuery] Guid? animalId,
        [FromQuery] Guid? vacinaId,
        [FromQuery] StatusComprovanteVacina? statusComprovante,
        CancellationToken cancellationToken)
    {
        var aplicacoes = await aplicacaoVacinaService.GetAtrasadasAsync(page, animalId, vacinaId, statusComprovante, cancellationToken);
        var responses = aplicacoes.Select(AplicacaoVacinaMapper.MapToHead).ToList();
        return Ok(responses);
    }

    [HttpGet("atrasadas/count")]
    public async Task<ActionResult<int>> CountAtrasadas(
        [FromQuery] Guid? animalId,
        [FromQuery] Guid? vacinaId,
        [FromQuery] StatusComprovanteVacina? statusComprovante,
        CancellationToken cancellationToken)
    {
        return Ok(await aplicacaoVacinaService.CountAtrasadasAsync(animalId, vacinaId, statusComprovante, cancellationToken));
    }

    [HttpGet("pendentes-assinatura")]
    public async Task<ActionResult<List<AplicacaoVacinaReadResponseDto>>> GetPendentesAssinatura(
        [FromQuery] int? page,
        [FromQuery] Guid? animalId,
        [FromQuery] Guid? vacinaId,
        [FromQuery] StatusComprovanteVacina? statusComprovante,
        CancellationToken cancellationToken)
    {
        var aplicacoes = await aplicacaoVacinaService.GetPendentesAssinaturaAsync(page, animalId, vacinaId, statusComprovante, cancellationToken);
        var responses = aplicacoes.Select(AplicacaoVacinaMapper.MapToHead).ToList();
        return Ok(responses);
    }

    [HttpGet("pendentes-assinatura/count")]
    public async Task<ActionResult<int>> CountPendentesAssinatura(
        [FromQuery] Guid? animalId,
        [FromQuery] Guid? vacinaId,
        [FromQuery] StatusComprovanteVacina? statusComprovante,
        CancellationToken cancellationToken)
    {
        return Ok(await aplicacaoVacinaService.CountPendentesAssinaturaAsync(animalId, vacinaId, statusComprovante, cancellationToken));
    }

    [HttpGet("proximas")]
    public async Task<ActionResult<List<AplicacaoVacinaReadResponseDto>>> GetProximas(
        [FromQuery] int? page,
        [FromQuery] DateOnly? dataLimite,
        [FromQuery] Guid? animalId,
        [FromQuery] Guid? vacinaId,
        CancellationToken cancellationToken)
    {
        var aplicacoes = await aplicacaoVacinaService.GetProximasAsync(page, dataLimite, animalId, vacinaId, cancellationToken);
        var responses = aplicacoes.Select(AplicacaoVacinaMapper.MapToHead).ToList();
        return Ok(responses);
    }

    [HttpGet("proximas/count")]
    public async Task<ActionResult<int>> CountProximas(
        [FromQuery] DateOnly? dataLimite,
        [FromQuery] Guid? animalId,
        [FromQuery] Guid? vacinaId,
        CancellationToken cancellationToken)
    {
        return Ok(await aplicacaoVacinaService.CountProximasAsync(dataLimite, animalId, vacinaId, cancellationToken));
    }

    [HttpPost("lote")]
    [Authorize(Policy = ClinicalPolicies.ManageVaccineApplications)]
    public async Task<ActionResult<List<AplicacaoVacinaReadResponseDto>>> VacinarLote(
        [FromBody] AplicacaoVacinaLoteCreateDto dto,
        CancellationToken cancellationToken)
    {
        var aplicacoes = await aplicacaoVacinaService.VacinarLoteAsync(dto, cancellationToken);
        var responses = aplicacoes.Select(AplicacaoVacinaMapper.MapToHead).ToList();
        return Ok(responses);
    }

    [HttpPost]
    [Authorize(Policy = ClinicalPolicies.ManageVaccineApplications)]
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
    [Authorize(Policy = ClinicalPolicies.ManageVaccineApplications)]
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

    [HttpPost("{id:guid}/assinar")]
    [Authorize(Policy = ClinicalPolicies.SignVaccineApplications)]
    public async Task<ActionResult<AplicacaoVacinaDetailResponseDto>> Assinar(Guid id,
        AplicacaoVacinaAssinaturaDto dto, CancellationToken cancellationToken)
    {
        var aplicacao = await aplicacaoVacinaService.AssinarAsync(id, dto, cancellationToken);
        if (aplicacao is null)
            return NotFound();

        var fullAplicacao = await aplicacaoVacinaService.GetByIdAsync(id, cancellationToken);
        if (fullAplicacao is null)
            return BadRequest();

        return Ok(AplicacaoVacinaMapper.MapToResponse(fullAplicacao));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = ClinicalPolicies.DeleteVaccineApplications)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await aplicacaoVacinaService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("animal/{animalId:guid}/comprovante-vacinas/dados")]
    public async Task<ActionResult<ComprovanteVacinasDadosResponseDto>> GetComprovanteVacinasDados(Guid animalId,
        [FromQuery] bool incluirNaoAssinadas = true, CancellationToken cancellationToken = default)
    {
        var result = await aplicacaoVacinaService.GetComprovanteVacinasDadosAsync(animalId, incluirNaoAssinadas,
            cancellationToken);
        if (result is null)
            return NotFound();

        return Ok(result);
    }
}
