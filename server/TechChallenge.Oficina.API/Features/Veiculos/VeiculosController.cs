using Microsoft.AspNetCore.Mvc;
using TechChallenge.Oficina.Application.Features.Veiculos.Commands;
using TechChallenge.Oficina.Application.Features.Veiculos.Queries;
using TechChallenge.Oficina.Application.Features.Veiculos.Services;
using TechChallenge.Oficina.Application.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.API.Features.Veiculos;

[ApiController]
[Route("api/veiculos")]
public sealed class VeiculosController : ControllerBase
{
    private readonly IVeiculoService _veiculoService;

    public VeiculosController(IVeiculoService veiculoService)
    {
        _veiculoService = veiculoService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(VeiculoViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post([FromBody] CriarVeiculoCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var veiculo = await _veiculoService.CriarAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = veiculo.Id }, veiculo);
        }
        catch (DomainException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VeiculoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new ObterVeiculoPorIdQuery { Id = id };
            var veiculo = await _veiculoService.ObterPorIdAsync(query, cancellationToken);
            return Ok(veiculo);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VeiculoViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] Guid? clienteId, CancellationToken cancellationToken)
    {
        var veiculos = await _veiculoService.ListarAsync(new ListarVeiculosQuery { ClienteId = clienteId }, cancellationToken);
        return Ok(veiculos);
    }

    [HttpPut]
    [ProducesResponseType(typeof(VeiculoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromBody] AtualizarVeiculoCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var veiculo = await _veiculoService.AtualizarAsync(command, cancellationToken);
            return Ok(veiculo);
        }
        catch (DomainException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _veiculoService.ExcluirAsync(new ExcluirVeiculoCommand { Id = id }, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
