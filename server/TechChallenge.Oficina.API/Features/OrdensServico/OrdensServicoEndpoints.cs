using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Application.Features.OrdensServico.Commands;
using TechChallenge.Oficina.Application.Features.OrdensServico.Queries;
using TechChallenge.Oficina.Application.Features.OrdensServico.Services;
using TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.API.Features.OrdensServico
{
    public class OrdensServicoEndpoints
    {
        private readonly IOrdemServicoService _ordemServicoService;

        public OrdensServicoEndpoints(IOrdemServicoService ordemServicoService)
        {
            _ordemServicoService = ordemServicoService;
        }

        public async Task<Results<CreatedAtRoute<OrdemServicoViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> Post(CriarOrdemServicoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.CriarAsync(command, cancellationToken);
                return TypedResults.CreatedAtRoute(ordemServico, "GetOrdemServicoById", new { id = ordemServico.Id });
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

        public async Task<Results<Ok<OrdemServicoViewModel>, NotFound<Dictionary<string, string?>>>> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.ObterPorIdAsync(new ObterOrdemServicoPorIdQuery { Id = id }, cancellationToken);
                return TypedResults.Ok(ordemServico);
            }
            catch (KeyNotFoundException exception)
            {
                return TypedResults.NotFound(CriarErro(exception.Message));
            }
        }

        public async Task<Ok<IReadOnlyCollection<OrdemServicoViewModel>>> Get(CancellationToken cancellationToken)
        {
            var ordensServico = await _ordemServicoService.ListarAsync(new ListarOrdensServicoQuery(), cancellationToken);
            return TypedResults.Ok(ordensServico);
        }

        public async Task<Results<Ok<AcompanhamentoOrdemServicoViewModel>, NotFound<Dictionary<string, string?>>>> GetAcompanhamento(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var acompanhamento = await _ordemServicoService.ObterAcompanhamentoAsync(new ObterAcompanhamentoOrdemServicoPorIdQuery { Id = id }, cancellationToken);
                return TypedResults.Ok(acompanhamento);
            }
            catch (KeyNotFoundException exception)
            {
                return TypedResults.NotFound(CriarErro(exception.Message));
            }
        }

        public async Task<Results<Ok<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>>, NotFound<Dictionary<string, string?>>>> GetByCliente(Guid clienteId, CancellationToken cancellationToken)
        {
            try
            {
                var acompanhamentos = await _ordemServicoService.ListarPorClienteAsync(new ListarOrdensServicoPorClienteQuery { ClienteId = clienteId }, cancellationToken);
                return TypedResults.Ok(acompanhamentos);
            }
            catch (KeyNotFoundException exception)
            {
                return TypedResults.NotFound(CriarErro(exception.Message));
            }
        }

        public async Task<Results<Ok<OrdemServicoViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> Put(AtualizarOrdemServicoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AtualizarAsync(command, cancellationToken);
                return TypedResults.Ok(ordemServico);
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
                await _ordemServicoService.ExcluirAsync(new ExcluirOrdemServicoCommand { Id = id }, cancellationToken);
                return TypedResults.NoContent();
            }
            catch (KeyNotFoundException exception)
            {
                return TypedResults.NotFound(CriarErro(exception.Message));
            }
        }

        public async Task<Results<Ok<OrdemServicoViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> AlterarParaEmDiagnostico(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AlterarStatusParaEmDiagnosticoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                return TypedResults.Ok(ordemServico);
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

        public async Task<Results<Ok<OrdemServicoViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> AlterarParaEmExecucao(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AlterarStatusParaEmExecucaoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                return TypedResults.Ok(ordemServico);
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

        public async Task<Results<Ok<OrdemServicoViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> AlterarParaFinalizada(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AlterarStatusParaFinalizadaAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                return TypedResults.Ok(ordemServico);
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

        public async Task<Results<Ok<OrdemServicoViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> AlterarParaEntregue(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AlterarStatusParaEntregueAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                return TypedResults.Ok(ordemServico);
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

        public async Task<Results<Ok<OrdemServicoViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> GerarOrcamento(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.GerarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                return TypedResults.Ok(ordemServico);
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

        public async Task<Results<NoContent, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> EnviarOrcamento(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _ordemServicoService.EnviarOrcamentoPorEmailAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                return TypedResults.NoContent();
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

        public async Task<Results<Ok<OrdemServicoViewModel>, BadRequest<Dictionary<string, string?>>, NotFound<Dictionary<string, string?>>>> AprovarOrcamento(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AprovarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                return TypedResults.Ok(ordemServico);
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

        private static Dictionary<string, string?> CriarErro(string? message)
        {
            return new Dictionary<string, string?>
            {
                ["message"] = message
            };
        }
    }
}
