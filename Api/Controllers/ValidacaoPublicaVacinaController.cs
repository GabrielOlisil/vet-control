using Api.DTOs;
using Api.Mappers;
using Api.Services.AplicacoesVacina;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/validacao-vacinacao")]
public sealed class ValidacaoPublicaVacinaController(IAplicacaoVacinaService aplicacaoVacinaService) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<ValidacaoPublicaVacinaResponseDto>> Consultar(
        Guid id, CancellationToken cancellationToken)
    {
        var aplicacao = await aplicacaoVacinaService.GetByIdAsync(id, cancellationToken);
        if (aplicacao is null)
        {
            return Ok(new ValidacaoPublicaVacinaResponseDto
            {
                Encontrado = false,
                CodigoValidacao = id,
                Mensagem = "Nenhuma vacinação foi encontrada para o código informado."
            });
        }

        var validacao = await aplicacaoVacinaService.ValidarAssinaturaAsync(id, cancellationToken);
        if (validacao is null)
            return NotFound();

        return Ok(new ValidacaoPublicaVacinaResponseDto
        {
            Encontrado = true,
            CodigoValidacao = id,
            Mensagem = validacao.Mensagem,
            Assinada = validacao.Assinada,
            AssinaturaIntegra = validacao.Integra,
            AuditoriaIntegra = validacao.AuditoriaIntegra,
            Validacao = validacao,
            Registro = AplicacaoVacinaMapper.MapToResponse(aplicacao)
        });
    }
}
