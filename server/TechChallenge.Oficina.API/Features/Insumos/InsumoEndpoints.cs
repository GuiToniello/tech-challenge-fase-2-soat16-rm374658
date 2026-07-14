using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.Queries;
using TechChallenge.Oficina.Application.Features.Insumos.Services;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.API.Features.Insumos
{
    public class InsumoEndpoints
    {
        private readonly IInsumoService _insumoService;

        public InsumoEndpoints(IInsumoService insumoService)
        {
            _insumoService = insumoService;
        }

        public async Task<Results<CreatedAtRoute<InsumoViewModel>, BadRequest<Dictionary<string, string?>>>> Post(CriarInsumoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var insumo = await _insumoService.CriarAsync(command, cancellationToken);
                return TypedResults.CreatedAtRoute(insumo, "GetInsumoById", new { id = insumo.Id });
            }
            catch (DomainException exception)
            {
                return TypedResults.BadRequest(CriarErro(exception.Message));
            }
        }

        public async Task<Results<Ok<InsumoViewModel>, NotFound<Dictionary<string, string?>>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterInsumoPorIdQuery { Id = id };
                var insumo = await _insumoService.ObterPorIdAsync(query, cancellationToken);
                return TypedResults.Ok(insumo);
            }
            catch (KeyNotFoundException exception)
            {
                return TypedResults.NotFound(CriarErro(exception.Message));
            }
        }

        public async Task<Ok<IReadOnlyCollection<InsumoViewModel>>> Get(CancellationToken cancellationToken)
        {
            var insumos = await _insumoService.ListarAsync(new ListarInsumosQuery(), cancellationToken);
            return TypedResults.Ok(insumos);
        }

        public async Task<Results<Ok<InsumoViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> Put(AtualizarInsumoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var insumo = await _insumoService.AtualizarAsync(command, cancellationToken);
                return TypedResults.Ok(insumo);
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
                var command = new ExcluirInsumoCommand { Id = id };
                await _insumoService.ExcluirAsync(command, cancellationToken);
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
