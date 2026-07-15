using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.Queries;
using TechChallenge.Oficina.Application.Features.Insumos.Services;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.Controllers.Features.Insumos
{
    public class InsumoController : IInsumoController
    {
        private readonly IInsumoService _insumoService;
        private readonly IInsumoAdapter _insumoAdapter;

        public InsumoController(IInsumoService insumoService, IInsumoAdapter insumoAdapter)
        {
            _insumoService = insumoService;
            _insumoAdapter = insumoAdapter;
        }

        public async Task<object> Post(CriarInsumoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var insumo = await _insumoService.CriarAsync(command, cancellationToken);
                var result = InsumoResult.From(insumo);

                return _insumoAdapter.Adapt(result, true);
            }
            catch (DomainException exception)
            {
                var result = InsumoResult.FromError<InsumoViewModel>(exception);

                return _insumoAdapter.Adapt(result);
            }
        }

        public async Task<object> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterInsumoPorIdQuery { Id = id };
                var insumo = await _insumoService.ObterPorIdAsync(query, cancellationToken);
                var result = InsumoResult.From(insumo);

                return _insumoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = InsumoResult.FromError<InsumoViewModel>(exception);
                return _insumoAdapter.Adapt(result);
            }
        }

        public async Task<object> Get(CancellationToken cancellationToken)
        {
            var insumos = await _insumoService.ListarAsync(new ListarInsumosQuery(), cancellationToken);
            var result = InsumoResult.From(insumos);

            return _insumoAdapter.Adapt(result);
        }

        public async Task<object> Put(AtualizarInsumoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var insumo = await _insumoService.AtualizarAsync(command, cancellationToken);
                var result = InsumoResult.From(insumo);
                return _insumoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = InsumoResult.FromError<InsumoViewModel>(exception);
                return _insumoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = InsumoResult.FromError<InsumoViewModel>(exception);
                return _insumoAdapter.Adapt(result);
            }
        }

        public async Task<object> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ExcluirInsumoCommand { Id = id };
                await _insumoService.ExcluirAsync(command, cancellationToken);

                return _insumoAdapter.Empty();
            }
            catch (KeyNotFoundException exception)
            {
                var result = InsumoResult.FromError<bool>(exception);
                return _insumoAdapter.Adapt(result);
            }
        }
    }
}
