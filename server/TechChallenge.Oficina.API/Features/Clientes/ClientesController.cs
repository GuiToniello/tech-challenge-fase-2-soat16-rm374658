using Microsoft.AspNetCore.Mvc;
using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.Queries;
using TechChallenge.Oficina.Application.Features.Clientes.Services;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.API.Features.Clientes;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClienteViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CriarClienteCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var cliente = await _clienteService.CriarAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
        }
        catch (DomainException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClienteViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new ObterClientePorIdQuery { Id = id };
            var cliente = await _clienteService.ObterPorIdAsync(query, cancellationToken);
            return Ok(cliente);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var clientes = await _clienteService.ListarAsync(new ListarClientesQuery(), cancellationToken);
        return Ok(clientes);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ClienteViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromBody] AtualizarClienteCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var cliente = await _clienteService.AtualizarAsync(command, cancellationToken);
            return Ok(cliente);
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
            var command = new ExcluirClienteCommand { Id = id };
            await _clienteService.ExcluirAsync(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
