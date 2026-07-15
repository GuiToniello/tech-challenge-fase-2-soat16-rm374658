using TechChallenge.Oficina.Application.Features.Servicos.Commands;
using TechChallenge.Oficina.Application.Features.Servicos.Queries;
using TechChallenge.Oficina.Application.Features.Servicos.Services;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.Controllers.Features.Servicos
{
    public class ServicoController
    {
        private readonly IServicoService _servicoService;
        private readonly IServicoAdapter _servicoAdapter;

        public ServicoController(IServicoService servicoService, IServicoAdapter servicoAdapter)
        {
            _servicoService = servicoService;
            _servicoAdapter = servicoAdapter;
        }

        public async Task<object> Post(CriarServicoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var servico = await _servicoService.CriarAsync(command, cancellationToken);
                var result = ServicoResult.From(servico);

                return _servicoAdapter.Adapt(result, true);
            }
            catch (DomainException exception)
            {
                var result = ServicoResult.FromError<ServicoViewModel>(exception);
                return _servicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = ServicoResult.FromError<ServicoViewModel>(exception);
                return _servicoAdapter.Adapt(result);
            }
        }

        public async Task<object> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterServicoPorIdQuery { Id = id };
                var servico = await _servicoService.ObterPorIdAsync(query, cancellationToken);
                var result = ServicoResult.From(servico);

                return _servicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = ServicoResult.FromError<ServicoViewModel>(exception);
                return _servicoAdapter.Adapt(result);
            }
        }

        public async Task<object> Get(CancellationToken cancellationToken)
        {
            var servicos = await _servicoService.ListarAsync(new ListarServicosQuery(), cancellationToken);
            var result = ServicoResult.From(servicos);

            return _servicoAdapter.Adapt(result);
        }

        public async Task<object> Put(AtualizarServicoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var servico = await _servicoService.AtualizarAsync(command, cancellationToken);
                var result = ServicoResult.From(servico);

                return _servicoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = ServicoResult.FromError<ServicoViewModel>(exception);
                return _servicoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = ServicoResult.FromError<ServicoViewModel>(exception);
                return _servicoAdapter.Adapt(result);
            }
        }

        public async Task<object> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ExcluirServicoCommand { Id = id };
                await _servicoService.ExcluirAsync(command, cancellationToken);

                return _servicoAdapter.Empty();
            }
            catch (KeyNotFoundException exception)
            {
                var result = ServicoResult.FromError<bool>(exception);
                return _servicoAdapter.Adapt(result);
            }
        }
    }
}
