using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.Queries;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;

namespace TechChallenge.Oficina.Application.Features.Clientes.Services;

public interface IClienteService
{
    Task<ClienteViewModel> CriarAsync(CriarClienteCommand command, CancellationToken cancellationToken = default);
    Task<ClienteViewModel> AtualizarAsync(AtualizarClienteCommand command, CancellationToken cancellationToken = default);
    Task<ClienteViewModel> ObterPorIdAsync(ObterClientePorIdQuery query, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ClienteViewModel>> ListarAsync(ListarClientesQuery query, CancellationToken cancellationToken = default);
    Task ExcluirAsync(ExcluirClienteCommand command, CancellationToken cancellationToken = default);
}
