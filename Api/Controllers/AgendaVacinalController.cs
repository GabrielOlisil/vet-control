using Api.DTOs;
using Api.Services.AgendaVacinal;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/agenda-vacinal")]
public sealed class AgendaVacinalController(AgendaVacinalService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AgendaVacinalItemDto>>> Get([FromQuery] AgendaVacinalSearchDto search,
        [FromQuery] int? page, CancellationToken ct) => Ok(await service.GetAsync(page, search, ct));

    [HttpGet("count")]
    public async Task<ActionResult<int>> Count([FromQuery] AgendaVacinalSearchDto search, CancellationToken ct) =>
        Ok(await service.CountAsync(search, ct));
}
