using Api.DTOs;
using Api.Mappers;
using Api.Models.Enums;
using Api.Services.AplicacoesVacina;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[AllowAnonymous]
[EnableRateLimiting("public-validation")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
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
                Mensagem = "Nenhuma vacinação foi encontrada para o código informado."
            });
        }

        var validacao = await aplicacaoVacinaService.ValidarAssinaturaAsync(id, cancellationToken);
        if (validacao is null)
            return NotFound();

        var registro = AplicacaoVacinaMapper.MapToResponse(aplicacao);
        return Ok(new ValidacaoPublicaVacinaResponseDto
        {
            SituacaoRegistro = aplicacao.SituacaoRegistro,
            RegistroVigente = aplicacao.SituacaoRegistro == SituacaoRegistroVacina.Ativo,
            DadosHistoricosPreservados = registro.DadosHistoricosPreservados,
            SubstituidaPorId = registro.SubstituidaPorId, RetificaAplicacaoVacinaId = registro.RetificaAplicacaoVacinaId,
            MotivoAlteracao = aplicacao.MotivoAlteracao,
            Encontrado = true,
            Mensagem = validacao.Mensagem,
            Assinada = validacao.Assinada,
            AssinaturaIntegra = validacao.Integra,
            VacinaNome = registro.Vacina?.Name,
            AnimalIdentificacao = registro.Animal?.Name ?? registro.Animal?.IdentificadorPrincipal?.Valor,
            DataAplicacao = aplicacao.DataAplicacao,
            NumeroLote = aplicacao.NumeroLote,
            VeterinarioResponsavel = aplicacao.VeterinarioResponsavel,
            VeterinarioCrmv = aplicacao.VeterinarioCrmv,
            VeterinarioCrmvUf = aplicacao.VeterinarioCrmvUf,
            Assinante = aplicacao.Assinante,
            AssinanteCrmv = aplicacao.AssinanteCrmv,
            AssinanteCrmvUf = aplicacao.AssinanteCrmvUf,
            AssinadaEm = aplicacao.AssinadaEm
        });
    }
}
