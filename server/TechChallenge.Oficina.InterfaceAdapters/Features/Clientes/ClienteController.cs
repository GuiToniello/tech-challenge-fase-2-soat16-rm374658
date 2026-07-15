using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.Queries;
using TechChallenge.Oficina.Application.Features.Clientes.Services;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.Controllers.Features.Clientes
{
    public class ClienteController
    {
        private readonly IClienteService _clienteService;
        private readonly IClientAdapter _clientAdapter;

        public ClienteController(IClienteService clienteService, IClientAdapter clientAdapter)
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

                return _clientAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = ClienteResult.FromError<ClienteViewModel>(exception);

                return _clientAdapter.Adapt(result);
            }
        }

        public async Task<ClienteResult<ClienteViewModel, KeyNotFoundException>> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterClientePorIdQuery { Id = id };
                var cliente = await _clienteService.ObterPorIdAsync(query, cancellationToken);
                return new ClienteResult<ClienteViewModel, KeyNotFoundException>(cliente);
            }
            catch (KeyNotFoundException exception)
            {
                return new ClienteResult<ClienteViewModel, KeyNotFoundException>(exception);
            }
        }

        public async Task<ClienteResult<IReadOnlyCollection<ClienteViewModel>, DomainException>> Get(CancellationToken cancellationToken)
        {
            var clientes = await _clienteService.ListarAsync(new ListarClientesQuery(), cancellationToken);
            return new ClienteResult<IReadOnlyCollection<ClienteViewModel>, DomainException>(clientes);
        }

        public async Task<ClienteResult<ClienteViewModel, Exception>> Put(AtualizarClienteCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var cliente = await _clienteService.AtualizarAsync(command, cancellationToken);
                return new ClienteResult<ClienteViewModel, Exception>(cliente);
            }
            catch (DomainException exception)
            {
                return new ClienteResult<ClienteViewModel, Exception>(exception);
            }
            catch (KeyNotFoundException exception)
            {
                return new ClienteResult<ClienteViewModel, Exception>(exception);
            }
        }

        public async Task<ClienteResult<Boolean, KeyNotFoundException>> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ExcluirClienteCommand { Id = id };
                await _clienteService.ExcluirAsync(command, cancellationToken);
                return new ClienteResult<Boolean, KeyNotFoundException>(true);
            }
            catch (KeyNotFoundException exception)
            {
                return new ClienteResult<Boolean, KeyNotFoundException>(exception);
            }
        }
    }
}
