using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Application.Features.Servicos.Commands;
using TechChallenge.Oficina.Application.Features.Servicos.Queries;
using TechChallenge.Oficina.Application.Features.Servicos.Services;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.API.Features.Servicos
{
    public class ServicoEndpoints
    {
        private readonly IServicoService _servicoService;

        public ServicoEndpoints(IServicoService servicoService)
        {
            _servicoService = servicoService;
        }

        public async Task<Results<CreatedAtRoute<ServicoViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> Post(CriarServicoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var servico = await _servicoService.CriarAsync(command, cancellationToken);
                return TypedResults.CreatedAtRoute(servico, "GetServicoById", new { id = servico.Id });
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

        public async Task<Results<Ok<ServicoViewModel>, NotFound<Dictionary<string, string?>>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterServicoPorIdQuery { Id = id };
                var servico = await _servicoService.ObterPorIdAsync(query, cancellationToken);
                return TypedResults.Ok(servico);
            }
            catch (KeyNotFoundException exception)
            {
                return TypedResults.NotFound(CriarErro(exception.Message));
            }
        }

        public async Task<Ok<IReadOnlyCollection<ServicoViewModel>>> Get(CancellationToken cancellationToken)
        {
            var servicos = await _servicoService.ListarAsync(new ListarServicosQuery(), cancellationToken);
            return TypedResults.Ok(servicos);
        }

        public async Task<Results<Ok<ServicoViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> Put(AtualizarServicoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var servico = await _servicoService.AtualizarAsync(command, cancellationToken);
                return TypedResults.Ok(servico);
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
                var command = new ExcluirServicoCommand { Id = id };
                await _servicoService.ExcluirAsync(command, cancellationToken);
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
