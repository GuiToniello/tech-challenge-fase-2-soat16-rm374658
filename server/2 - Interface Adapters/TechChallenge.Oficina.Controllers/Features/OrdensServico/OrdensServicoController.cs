using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Queries;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Entities.Exceptions;

namespace TechChallenge.Oficina.Controllers.Features.OrdensServico
{
    public class OrdensServicoController : IOrdensServicoController
    {
        private readonly IOrdemServicoUseCases _ordemServicoService;
        private readonly IOrdensServicoAdapter _ordensServicoAdapter;

        public OrdensServicoController(IOrdemServicoUseCases ordemServicoUseCases, IOrdensServicoAdapter ordensServicoAdapter)
        {
            _ordemServicoService = ordemServicoUseCases;
            _ordensServicoAdapter = ordensServicoAdapter;
        }

        public async Task<object> Post(CriarOrdemServicoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.CriarAsync(command, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result, true);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);

                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterOrdemServicoPorIdQuery { Id = id };
                var ordemServico = await _ordemServicoService.ObterPorIdAsync(query, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> Get(CancellationToken cancellationToken)
        {
            var ordensServico = await _ordemServicoService.ListarAsync(new ListarOrdensServicoQuery(), cancellationToken);
            var result = OrdensServicoResult.From(ordensServico);

            return _ordensServicoAdapter.Adapt(result);
        }

        public async Task<object> GetAcompanhamento(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var acompanhamento = await _ordemServicoService.ObterAcompanhamentoAsync(new ObterAcompanhamentoOrdemServicoPorIdQuery { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(acompanhamento);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<AcompanhamentoOrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> GetByCliente(Guid clienteId, CancellationToken cancellationToken)
        {
            try
            {
                var acompanhamentos = await _ordemServicoService.ListarPorClienteAsync(new ListarOrdensServicoPorClienteQuery { ClienteId = clienteId }, cancellationToken);
                var result = OrdensServicoResult.From(acompanhamentos);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> Put(AtualizarOrdemServicoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AtualizarAsync(command, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);

                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ExcluirOrdemServicoCommand { Id = id };
                await _ordemServicoService.ExcluirAsync(command, cancellationToken);

                return _ordensServicoAdapter.Empty();
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<bool>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> AlterarParaEmDiagnostico(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AlterarStatusParaEmDiagnosticoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> AlterarParaEmExecucao(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AlterarStatusParaEmExecucaoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> AlterarParaFinalizada(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AlterarStatusParaFinalizadaAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> AlterarParaEntregue(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AlterarStatusParaEntregueAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> GerarOrcamento(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.GerarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> EnviarOrcamento(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _ordemServicoService.EnviarOrcamentoPorEmailAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);

                return _ordensServicoAdapter.Empty();
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<bool>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<bool>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }

        public async Task<object> AprovarOrcamento(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var ordemServico = await _ordemServicoService.AprovarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
                var result = OrdensServicoResult.From(ordemServico);

                return _ordensServicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = OrdensServicoResult.FromError<OrdemServicoViewModel>(exception);
                return _ordensServicoAdapter.Adapt(result);
            }
        }
    }
}
