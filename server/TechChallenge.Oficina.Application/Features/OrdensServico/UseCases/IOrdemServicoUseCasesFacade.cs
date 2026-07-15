using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.Orcamentos;
using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;

namespace TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;

public interface IOrdemServicoUseCasesFacade
{
    IEstoqueUseCases EstoqueService { get; }
    IIndicadorUseCases IndicadorService { get; }
    IOrcamentoEmailSender OrcamentoEmailSender { get; }
}
