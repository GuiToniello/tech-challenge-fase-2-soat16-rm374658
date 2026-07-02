using TechChallenge.Oficina.Application.Features.Indicadores;
using TechChallenge.Oficina.Domain.Features.Insumos;
using TechChallenge.Oficina.Domain.Features.Orcamentos;

namespace TechChallenge.Oficina.Application.Features.OrdensServico.Services;

public interface IOrdemServicoServicesFacade
{
    IEstoqueService EstoqueService { get; }
    IIndicadorService IndicadorService { get; }
    IOrcamentoEmailSender OrcamentoEmailSender { get; }
}
