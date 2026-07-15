using TechChallenge.Oficina.Application.Features.Veiculos.Commands;
using TechChallenge.Oficina.Application.Features.Veiculos.Queries;
using TechChallenge.Oficina.Application.Features.Veiculos.Services;
using TechChallenge.Oficina.Application.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.Controllers.Features.Veiculos
{
    public class VeiculoController
    {
        private readonly IVeiculoService _veiculoService;
        private readonly IVeiculoAdapter _veiculoAdapter;

        public VeiculoController(IVeiculoService veiculoService, IVeiculoAdapter veiculoAdapter)
        {
            _veiculoService = veiculoService;
            _veiculoAdapter = veiculoAdapter;
        }

        public async Task<object> Post(CriarVeiculoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var veiculo = await _veiculoService.CriarAsync(command, cancellationToken);
                var result = VeiculoResult.From(veiculo);

                return _veiculoAdapter.Adapt(result, true);
            }
            catch (DomainException exception)
            {
                var result = VeiculoResult.FromError<VeiculoViewModel>(exception);
                return _veiculoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = VeiculoResult.FromError<VeiculoViewModel>(exception);
                return _veiculoAdapter.Adapt(result);
            }
        }

        public async Task<object> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new ObterVeiculoPorIdQuery { Id = id };
                var veiculo = await _veiculoService.ObterPorIdAsync(query, cancellationToken);
                var result = VeiculoResult.From(veiculo);

                return _veiculoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = VeiculoResult.FromError<VeiculoViewModel>(exception);
                return _veiculoAdapter.Adapt(result);
            }
        }

        public async Task<object> Get(Guid? clienteId, CancellationToken cancellationToken)
        {
            var query = new ListarVeiculosQuery { ClienteId = clienteId };
            var veiculos = await _veiculoService.ListarAsync(query, cancellationToken);
            var result = VeiculoResult.From(veiculos);

            return _veiculoAdapter.Adapt(result);
        }

        public async Task<object> Put(AtualizarVeiculoCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var veiculo = await _veiculoService.AtualizarAsync(command, cancellationToken);
                var result = VeiculoResult.From(veiculo);

                return _veiculoAdapter.Adapt(result);
            }
            catch (DomainException exception)
            {
                var result = VeiculoResult.FromError<VeiculoViewModel>(exception);
                return _veiculoAdapter.Adapt(result);
            }
            catch (KeyNotFoundException exception)
            {
                var result = VeiculoResult.FromError<VeiculoViewModel>(exception);
                return _veiculoAdapter.Adapt(result);
            }
        }

        public async Task<object> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ExcluirVeiculoCommand { Id = id };
                await _veiculoService.ExcluirAsync(command, cancellationToken);

                return _veiculoAdapter.Empty();
            }
            catch (KeyNotFoundException exception)
            {
                var result = VeiculoResult.FromError<bool>(exception);
                return _veiculoAdapter.Adapt(result);
            }
        }
    }
}
