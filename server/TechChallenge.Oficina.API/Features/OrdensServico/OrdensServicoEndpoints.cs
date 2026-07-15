using TechChallenge.Oficina.Application.Features.OrdensServico.Commands;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;

namespace TechChallenge.Oficina.API.Features.OrdensServico
{
    public class OrdensServicoEndpoints
    {
        private readonly OrdensServicoController _ordensServicoController;

        public OrdensServicoEndpoints(OrdensServicoController ordensServicoController)
        {
            _ordensServicoController = ordensServicoController;
        }

        public async Task<object> Post(CriarOrdemServicoCommand command, CancellationToken cancellationToken)
            => await _ordensServicoController.Post(command, cancellationToken);

        public async Task<object> GetById(Guid id, CancellationToken cancellationToken)
            => await _ordensServicoController.GetById(id, cancellationToken);

        public async Task<object> Get(CancellationToken cancellationToken)
            => await _ordensServicoController.Get(cancellationToken);

        public async Task<object> GetAcompanhamento(Guid id, CancellationToken cancellationToken)
            => await _ordensServicoController.GetAcompanhamento(id, cancellationToken);

        public async Task<object> GetByCliente(Guid clienteId, CancellationToken cancellationToken)
            => await _ordensServicoController.GetByCliente(clienteId, cancellationToken);

        public async Task<object> Put(AtualizarOrdemServicoCommand command, CancellationToken cancellationToken)
            => await _ordensServicoController.Put(command, cancellationToken);

        public async Task<object> Delete(Guid id, CancellationToken cancellationToken)
            => await _ordensServicoController.Delete(id, cancellationToken);

        public async Task<object> AlterarParaEmDiagnostico(Guid id, CancellationToken cancellationToken)
            => await _ordensServicoController.AlterarParaEmDiagnostico(id, cancellationToken);

        public async Task<object> AlterarParaEmExecucao(Guid id, CancellationToken cancellationToken)
            => await _ordensServicoController.AlterarParaEmExecucao(id, cancellationToken);

        public async Task<object> AlterarParaFinalizada(Guid id, CancellationToken cancellationToken)
            => await _ordensServicoController.AlterarParaFinalizada(id, cancellationToken);

        public async Task<object> AlterarParaEntregue(Guid id, CancellationToken cancellationToken)
            => await _ordensServicoController.AlterarParaEntregue(id, cancellationToken);

        public async Task<object> GerarOrcamento(Guid id, CancellationToken cancellationToken)
            => await _ordensServicoController.GerarOrcamento(id, cancellationToken);

        public async Task<object> EnviarOrcamento(Guid id, CancellationToken cancellationToken)
            => await _ordensServicoController.EnviarOrcamento(id, cancellationToken);

        public async Task<object> AprovarOrcamento(Guid id, CancellationToken cancellationToken)
            => await _ordensServicoController.AprovarOrcamento(id, cancellationToken);
    }
}
