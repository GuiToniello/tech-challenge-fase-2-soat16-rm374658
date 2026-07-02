using TechChallenge.Oficina.Domain.Features.OrdensServico;

namespace TechChallenge.Oficina.Domain.Features.Orcamentos;

public interface IOrcamentoEmailSender
{
    Task EnviarOrcamentoAsync(OrdemServico ordemServico, string emailDestino, CancellationToken cancellationToken = default);
}
