using TechChallenge.Oficina.Application.Features.OrdensServico.Commands;
using TechChallenge.Oficina.Application.Features.OrdensServico.Queries;
using TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;

namespace TechChallenge.Oficina.Application.Features.OrdensServico.Services;

public interface IOrdemServicoService
{
    Task<OrdemServicoViewModel> CriarAsync(CriarOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> AtualizarAsync(AtualizarOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> ObterPorIdAsync(ObterOrdemServicoPorIdQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OrdemServicoViewModel>> ListarAsync(ListarOrdensServicoQuery query, CancellationToken cancellationToken = default);
    Task ExcluirAsync(ExcluirOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<AcompanhamentoOrdemServicoViewModel> ObterAcompanhamentoAsync(ObterAcompanhamentoOrdemServicoPorIdQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>> ListarPorClienteAsync(ListarOrdensServicoPorClienteQuery query, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> AlterarStatusParaEmDiagnosticoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> GerarOrcamentoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task EnviarOrcamentoPorEmailAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> AprovarOrcamentoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> AlterarStatusParaEmExecucaoAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> AlterarStatusParaFinalizadaAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
    Task<OrdemServicoViewModel> AlterarStatusParaEntregueAsync(AlterarStatusOrdemServicoCommand command, CancellationToken cancellationToken = default);
}
