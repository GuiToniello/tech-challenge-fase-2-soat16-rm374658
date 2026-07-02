using TechChallenge.Oficina.Application.Features.Veiculos.Commands;
using TechChallenge.Oficina.Application.Features.Veiculos.Queries;
using TechChallenge.Oficina.Application.Features.Veiculos.ViewModels;

namespace TechChallenge.Oficina.Application.Features.Veiculos.Services;

public interface IVeiculoService
{
    Task<VeiculoViewModel> CriarAsync(CriarVeiculoCommand command, CancellationToken cancellationToken = default);
    Task<VeiculoViewModel> AtualizarAsync(AtualizarVeiculoCommand command, CancellationToken cancellationToken = default);
    Task<VeiculoViewModel> ObterPorIdAsync(ObterVeiculoPorIdQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<VeiculoViewModel>> ListarAsync(ListarVeiculosQuery query, CancellationToken cancellationToken = default);
    Task ExcluirAsync(ExcluirVeiculoCommand command, CancellationToken cancellationToken = default);
}
