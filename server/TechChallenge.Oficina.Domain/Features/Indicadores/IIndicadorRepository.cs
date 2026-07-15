namespace TechChallenge.Oficina.Entities.Features.Indicadores;

public interface IIndicadorRepository
{
    Task<Indicador?> ObterAsync(CancellationToken cancellationToken = default);
    Task SalvarAsync(Indicador indicador, CancellationToken cancellationToken = default);
}
