using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.Queries;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;

namespace TechChallenge.Oficina.Application.Features.Insumos.Services;

public interface IInsumoService
{
    Task<InsumoViewModel> CriarAsync(CriarInsumoCommand command, CancellationToken cancellationToken = default);
    Task<InsumoViewModel> AtualizarAsync(AtualizarInsumoCommand command, CancellationToken cancellationToken = default);
    Task<InsumoViewModel> ObterPorIdAsync(ObterInsumoPorIdQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InsumoViewModel>> ListarAsync(ListarInsumosQuery query, CancellationToken cancellationToken = default);
    Task ExcluirAsync(ExcluirInsumoCommand command, CancellationToken cancellationToken = default);
}
