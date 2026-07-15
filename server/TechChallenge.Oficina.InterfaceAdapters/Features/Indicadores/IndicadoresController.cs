using TechChallenge.Oficina.UseCases.Features.Indicadores.Queries;
using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;

namespace TechChallenge.Oficina.Controllers.Features.Indicadores
{
    public sealed class IndicadoresController : IIndicadoresController
    {
        private readonly IIndicadorUseCases _indicadorService;
        private readonly IIndicadoresAdapter _indicadoresAdapter;

        public IndicadoresController(IIndicadorUseCases indicadorService, IIndicadoresAdapter indicadoresAdapter)
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
