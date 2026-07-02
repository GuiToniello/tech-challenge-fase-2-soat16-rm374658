namespace TechChallenge.Oficina.Domain.Features.Insumos;

public interface IInsumoRepository
{
    Task AdicionarAsync(Insumo insumo, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Insumo insumo, CancellationToken cancellationToken = default);
    Task<Insumo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Insumo>> ListarAsync(CancellationToken cancellationToken = default);
    Task RemoverAsync(Insumo insumo, CancellationToken cancellationToken = default);
}
