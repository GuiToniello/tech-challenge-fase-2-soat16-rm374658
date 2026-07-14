using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Application.Features.Veiculos.Commands;
using TechChallenge.Oficina.Application.Features.Veiculos.Queries;
using TechChallenge.Oficina.Application.Features.Veiculos.Services;
using TechChallenge.Oficina.Application.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.API.Features.Veiculos
{
    public class VeiculoEndpoints
    {
        private readonly IVeiculoService _veiculoService;

        public VeiculoEndpoints(IVeiculoService veiculoService)
        {
            _veiculoService = veiculoService;
        }

        public async Task<Results<CreatedAtRoute<VeiculoViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> Post(CriarVeiculoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var veiculo = await _veiculoService.CriarAsync(command, cancellationToken);
                return TypedResults.CreatedAtRoute(veiculo, "GetVeiculoById", new { id = veiculo.Id });
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

        public async Task<Results<Ok<VeiculoViewModel>, NotFound<Dictionary<string, string?>>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterVeiculoPorIdQuery { Id = id };
                var veiculo = await _veiculoService.ObterPorIdAsync(query, cancellationToken);
                return TypedResults.Ok(veiculo);
            }
            catch (KeyNotFoundException exception)
            {
                return TypedResults.NotFound(CriarErro(exception.Message));
            }
        }

        public async Task<Ok<IEnumerable<VeiculoViewModel>>> Get(Guid? clienteId, CancellationToken cancellationToken)
        {
            var veiculos = await _veiculoService.ListarAsync(new ListarVeiculosQuery { ClienteId = clienteId }, cancellationToken);
            return TypedResults.Ok(veiculos);
        }

        public async Task<Results<Ok<VeiculoViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> Put(AtualizarVeiculoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var veiculo = await _veiculoService.AtualizarAsync(command, cancellationToken);
                return TypedResults.Ok(veiculo);
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
                await _veiculoService.ExcluirAsync(new ExcluirVeiculoCommand { Id = id }, cancellationToken);
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
