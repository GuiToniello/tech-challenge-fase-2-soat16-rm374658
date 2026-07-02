using Microsoft.AspNetCore.Mvc;
using TechChallenge.Oficina.Application.Features.Indicadores.Queries;
using TechChallenge.Oficina.Application.Features.Indicadores.ViewModels;

namespace TechChallenge.Oficina.API.Features.Indicadores;

[ApiController]
[Route("api/indicadores")]
public sealed class IndicadoresController : ControllerBase
{
    private readonly TechChallenge.Oficina.Application.Features.Indicadores.IIndicadorService _indicadorService;

    public IndicadoresController(TechChallenge.Oficina.Application.Features.Indicadores.IIndicadorService indicadorService)
    {
        _indicadorService = indicadorService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IndicadorViewModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var indicadores = await _indicadorService.ObterAsync(new ObterIndicadoresQuery(), cancellationToken);
        return Ok(indicadores);
    }
}
