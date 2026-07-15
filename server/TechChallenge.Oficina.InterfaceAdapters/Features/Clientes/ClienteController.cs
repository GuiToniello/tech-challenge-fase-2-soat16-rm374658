using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.Queries;
using TechChallenge.Oficina.Application.Features.Clientes.Services;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.Controllers.Features.Clientes
{
    public class ClienteController : IClienteController
    {
        private readonly IClienteService _clienteService;
        private readonly IClienteAdapter _clientAdapter;

        public ClienteController(IClienteService clienteService, IClienteAdapter clientAdapter)
        {
            _clienteService = clienteService;
            _clientAdapter = clientAdapter;
        }

        public async Task<object> Post(CriarClienteCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var cliente = await _clienteService.CriarAsync(command, cancellationToken);
                var result = ClienteResult.From(cliente);

                return _clientAdapter.Adapt(result, true);
            }
            catch (DomainException exception)
            {
                var result = ClienteResult.FromError<ClienteViewModel>(exception);

                return _clientAdapter.Adapt(result);
            }
        }

        public async Task<object> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterClientePorIdQuery { Id = id };
                var cliente = await _clienteService.ObterPorIdAsync(query, cancellationToken);
                var result = ClienteResult.From(cliente);

                return _clientAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = ClienteResult.FromError<ClienteViewModel>(exception);
                return _clientAdapter.Adapt(result);
            }
        }

        public async Task<object> Get(CancellationToken cancellationToken)
        {
            var clientes = await _clienteService.ListarAsync(new ListarClientesQuery(), cancellationToken);
            var result = ClienteResult.From(clientes);

            return _clientAdapter.Adapt(result);
        }

        public async Task<object> Put(AtualizarClienteCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var cliente = await _clienteService.AtualizarAsync(command, cancellationToken);
                var result = ClienteResult.From(cliente);
                return _clientAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = ClienteResult.FromError<ClienteViewModel>(exception);
                return _clientAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = ClienteResult.FromError<ClienteViewModel>(exception);
                return _clientAdapter.Adapt(result);
            }
        }

        public async Task<object> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ExcluirClienteCommand { Id = id };
                await _clienteService.ExcluirAsync(command, cancellationToken);

                return _clientAdapter.Empty();
            }
            catch (KeyNotFoundException exception)
            {
                var result = ClienteResult.FromError<bool>(exception);
                return _clientAdapter.Adapt(result);
            }
        }
    }
}
