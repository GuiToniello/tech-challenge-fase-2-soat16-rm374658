using TechChallenge.Oficina.Application.Features.Indicadores;
using TechChallenge.Oficina.Application.Features.Indicadores.Queries;

namespace TechChallenge.Oficina.Controllers.Features.Indicadores
{
    public sealed class IndicadoresController : IIndicadoresController
    {
        private readonly IIndicadorService _indicadorService;
        private readonly IIndicadoresAdapter _indicadoresAdapter;

        public IndicadoresController(IIndicadorService indicadorService, IIndicadoresAdapter indicadoresAdapter)
        {
            _indicadorService = indicadorService;
            _indicadoresAdapter = indicadoresAdapter;
        }

        public async Task<object> Get(CancellationToken cancellationToken)
        {
            var indicadores = await _indicadorService.ObterAsync(new ObterIndicadoresQuery(), cancellationToken);
            var result =  IndicadoresResult.From(indicadores);

            return _indicadoresAdapter.Adapt(result);
        }
    }
}
