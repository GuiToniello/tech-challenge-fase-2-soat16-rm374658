namespace TechChallenge.Oficina.Domain.Features.Indicadores;

public interface IIndicadorRepository
{
    Task<Indicador?> ObterAsync(CancellationToken cancellationToken = default);
    Task SalvarAsync(Indicador indicador, CancellationToken cancellationToken = default);
}
