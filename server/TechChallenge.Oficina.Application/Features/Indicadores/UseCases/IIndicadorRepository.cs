using TechChallenge.Oficina.Entities.Features.Indicadores;

namespace TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;

public interface IIndicadorRepository
{
    Task<Indicador?> ObterAsync(CancellationToken cancellationToken = default);
    Task SalvarAsync(Indicador indicador, CancellationToken cancellationToken = default);
}
