using Microsoft.AspNetCore.Mvc;
using TechChallenge.Oficina.Application.Features.Servicos.Commands;
using TechChallenge.Oficina.Application.Features.Servicos.Queries;
using TechChallenge.Oficina.Application.Features.Servicos.Services;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.API.Features.Servicos;

[ApiController]
[Route("api/servicos")]
public sealed class ServicosController : ControllerBase
{
    private readonly IServicoService _servicoService;

    public ServicosController(IServicoService servicoService)
    {
        _servicoService = servicoService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ServicoViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post([FromBody] CriarServicoCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var servico = await _servicoService.CriarAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = servico.Id }, servico);
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
    [ProducesResponseType(typeof(ServicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new ObterServicoPorIdQuery { Id = id };
            var servico = await _servicoService.ObterPorIdAsync(query, cancellationToken);
            return Ok(servico);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ServicoViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var servicos = await _servicoService.ListarAsync(new ListarServicosQuery(), cancellationToken);
        return Ok(servicos);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ServicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromBody] AtualizarServicoCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var servico = await _servicoService.AtualizarAsync(command, cancellationToken);
            return Ok(servico);
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
            var command = new ExcluirServicoCommand { Id = id };
            await _servicoService.ExcluirAsync(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }
}
