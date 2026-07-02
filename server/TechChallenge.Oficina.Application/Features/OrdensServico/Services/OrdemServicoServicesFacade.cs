using TechChallenge.Oficina.Application.Features.Indicadores;
using TechChallenge.Oficina.Domain.Features.Insumos;
using TechChallenge.Oficina.Domain.Features.Orcamentos;

namespace TechChallenge.Oficina.Application.Features.OrdensServico.Services;

// Este facade agrupa serviços auxiliares relacionados ao fluxo de ordem de serviço para reduzir a quantidade de dependências injetadas no OrdemServicoService e atender à regra S107 do SonarQube sem alterar o padrão arquitetural atual.
public sealed class OrdemServicoServicesFacade : IOrdemServicoServicesFacade
{
    public OrdemServicoServicesFacade(
        IEstoqueService estoqueService,
        IIndicadorService indicadorService,
        IOrcamentoEmailSender orcamentoEmailSender)
    {
        EstoqueService = estoqueService;
        IndicadorService = indicadorService;
        OrcamentoEmailSender = orcamentoEmailSender;
    }

    public IEstoqueService EstoqueService { get; }

    public IIndicadorService IndicadorService { get; }

    public IOrcamentoEmailSender OrcamentoEmailSender { get; }
}
