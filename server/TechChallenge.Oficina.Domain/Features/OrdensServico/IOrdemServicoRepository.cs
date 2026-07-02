using TechChallenge.Oficina.Domain.Features.OrdensServico.Enums;

namespace TechChallenge.Oficina.Domain.Features.OrdensServico;

public interface IOrdemServicoRepository
{
    Task AdicionarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
    Task AtualizarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
    Task<OrdemServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrdemServico>> ListarAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrdemServico>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrdemServico>> ListarPorStatusAsync(StatusOrdemServico status, CancellationToken cancellationToken = default);
    Task RemoverAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default);
}
