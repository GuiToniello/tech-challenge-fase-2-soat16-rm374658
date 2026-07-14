using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.Queries;
using TechChallenge.Oficina.Application.Features.Clientes.Services;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.API.Features.Clientes
{
    public class ClientEndpoints
    {
        private readonly IClienteService _clienteService;

        public ClientEndpoints(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        public async Task<Results<CreatedAtRoute<ClienteViewModel>, BadRequest<Dictionary<string, string?>>>> Post(CriarClienteCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var cliente = await _clienteService.CriarAsync(command, cancellationToken);
                return TypedResults.CreatedAtRoute(cliente, "GetClienteById", new { id = cliente.Id });
            }
            catch (DomainException exception)
            {
                return TypedResults.BadRequest(CriarErro(exception.Message));
            }
        }

        public async Task<Results<Ok<ClienteViewModel>, NotFound<Dictionary<string, string?>>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterClientePorIdQuery { Id = id };
                var cliente = await _clienteService.ObterPorIdAsync(query, cancellationToken);
                return TypedResults.Ok(cliente);
            }
            catch (KeyNotFoundException exception)
            {
                return TypedResults.NotFound(CriarErro(exception.Message));
            }
        }

        public async Task<Ok<IReadOnlyCollection<ClienteViewModel>>> Get(CancellationToken cancellationToken)
        {
            var clientes = await _clienteService.ListarAsync(new ListarClientesQuery(), cancellationToken);
            return TypedResults.Ok(clientes);
        }

        public async Task<Results<Ok<ClienteViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> Put(AtualizarClienteCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var cliente = await _clienteService.AtualizarAsync(command, cancellationToken);
                return TypedResults.Ok(cliente);
            }
            catch (DomainException exception)
            {
                return TypedResults.BadRequest(CriarErro(exception.Message));
            }
            catch (KeyNotFoundException exception)
            {
                return TypedResults.NotFound(CriarErro(exception.Message));
            }
        }

        public async Task<Results<NoContent, NotFound<Dictionary<string, string?>>>> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ExcluirClienteCommand { Id = id };
                await _clienteService.ExcluirAsync(command, cancellationToken);
                return TypedResults.NoContent();
            }
            catch (KeyNotFoundException exception)
            {
                return TypedResults.NotFound(CriarErro(exception.Message));
            }
        }

        private static Dictionary<string, string?> CriarErro(string? message)
        {
            return new Dictionary<string, string?>
            {
                ["message"] = message
            };
        }
    }
}
