using TechChallenge.Oficina.Entities.Features.OrdensServico;

namespace TechChallenge.Oficina.Entities.Features.Orcamentos;

public interface IOrcamentoEmailSender
{
    Task EnviarOrcamentoAsync(OrdemServico ordemServico, string emailDestino, CancellationToken cancellationToken = default);
}
