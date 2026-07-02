using TechChallenge.Oficina.Domain.Features.Servicos;

namespace TechChallenge.Oficina.Domain.Features.Insumos;

public interface IEstoqueService
{
    Task VerificarDisponibilidadeParaOrcamentoAsync(IReadOnlyCollection<Servico> servicos, CancellationToken cancellationToken = default);
    Task DebitarEstoqueParaOrdemServicoAsync(IReadOnlyCollection<Servico> servicos, CancellationToken cancellationToken = default);
}
