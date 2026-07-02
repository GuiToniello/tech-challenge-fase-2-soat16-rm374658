using TechChallenge.Oficina.Application.Features.Servicos.Commands;
using TechChallenge.Oficina.Application.Features.Servicos.Queries;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;

namespace TechChallenge.Oficina.Application.Features.Servicos.Services;

public interface IServicoService
{
    Task<ServicoViewModel> CriarAsync(CriarServicoCommand command, CancellationToken cancellationToken = default);
    Task<ServicoViewModel> AtualizarAsync(AtualizarServicoCommand command, CancellationToken cancellationToken = default);
    Task<ServicoViewModel> ObterPorIdAsync(ObterServicoPorIdQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ServicoViewModel>> ListarAsync(ListarServicosQuery query, CancellationToken cancellationToken = default);
    Task ExcluirAsync(ExcluirServicoCommand command, CancellationToken cancellationToken = default);
}
