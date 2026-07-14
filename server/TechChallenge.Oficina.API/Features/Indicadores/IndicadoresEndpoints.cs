using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Application.Features.Indicadores.Queries;
using TechChallenge.Oficina.Application.Features.Indicadores.ViewModels;

namespace TechChallenge.Oficina.API.Features.Indicadores
{
    public sealed class IndicadoresEndpoints
    {
        private readonly TechChallenge.Oficina.Application.Features.Indicadores.IIndicadorService _indicadorService;

        public IndicadoresEndpoints(TechChallenge.Oficina.Application.Features.Indicadores.IIndicadorService indicadorService)
        {
            _indicadorService = indicadorService;
        }

        public async Task<Ok<IndicadorViewModel>> Get(CancellationToken cancellationToken)
        {
            var indicadores = await _indicadorService.ObterAsync(new ObterIndicadoresQuery(), cancellationToken);
            return TypedResults.Ok(indicadores);
        }
    }
}
