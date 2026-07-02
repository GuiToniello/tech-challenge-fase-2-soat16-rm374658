namespace TechChallenge.Oficina.Domain.Features.Indicadores;

public interface IIndicadorService<TObterQuery, TViewModel>
{
    Task<TViewModel> ObterAsync(TObterQuery query, CancellationToken cancellationToken = default);
    Task AtualizarAsync(CancellationToken cancellationToken = default);
}
