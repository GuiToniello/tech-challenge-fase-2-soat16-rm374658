using TechChallenge.Oficina.Entities.Features.Servicos;

namespace TechChallenge.Oficina.Entities.Features.Insumos;

public interface IEstoqueUseCases
{
    Task VerificarDisponibilidadeParaOrcamentoAsync(IReadOnlyCollection<Servico> servicos, CancellationToken cancellationToken = default);
    Task DebitarEstoqueParaOrdemServicoAsync(IReadOnlyCollection<Servico> servicos, CancellationToken cancellationToken = default);
}
