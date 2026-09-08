using Api.DTOs.CartoesVacina;
using Api.DTOs;
using Api.Mappers;
using Api.Services.CartoesVacina;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/cartoes-vacina")]
public class CartaoVacinaController(ICartaoVacinaService cartaoVacinaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CartaoVacinaReadResponseDto>>> GetAll(
        [FromQuery] CartaoVacinaSearchDto search, CancellationToken cancellationToken)
    {
        var cartoes = await cartaoVacinaService.GetAllAsync(search, cancellationToken);
        var responses = cartoes.Select(CartaoVacinaMapper.MapToHead).ToList();
        return Ok(responses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CartaoVacinaDetailResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cartao = await cartaoVacinaService.GetByIdAsync(id, cancellationToken);
        if (cartao is null)
            return NotFound();

        var response = CartaoVacinaMapper.MapToResponse(cartao);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CartaoVacinaDetailResponseDto>> Create(CartaoVacinaCreateDto dto,
        CancellationToken cancellationToken)
    {
        var cartao = await cartaoVacinaService.CreateAsync(dto, cancellationToken);
        var fullCartao = await cartaoVacinaService.GetByIdAsync(cartao.Id, cancellationToken);
        if (fullCartao is null)
            return BadRequest();

        var response = CartaoVacinaMapper.MapToResponse(fullCartao);
        return CreatedAtAction(nameof(GetById), new { id = cartao.Id }, response);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<CartaoVacinaDetailResponseDto>> Patch(Guid id, CartaoVacinaPatchDto dto,
        CancellationToken cancellationToken)
    {
        var cartao = await cartaoVacinaService.PatchAsync(id, dto, cancellationToken);
        if (cartao is null)
            return NotFound();

        var fullCartao = await cartaoVacinaService.GetByIdAsync(id, cancellationToken);
        if (fullCartao is null)
            return BadRequest();

        var response = CartaoVacinaMapper.MapToResponse(fullCartao);
        return Ok(response);
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> Count(CancellationToken cancellationToken)
    {
        return Ok(await cartaoVacinaService.Count(cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await cartaoVacinaService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}