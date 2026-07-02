using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.Queries;
using TechChallenge.Oficina.Application.Features.Insumos.Services;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.API.Features.Insumos;

[ApiController]
[Route("api/insumos")]
[Produces("application/json")]
public sealed class InsumosController : ControllerBase
{
    private const string MensagemErroInterno = "Ocorreu um erro interno.";

    private readonly IInsumoService _insumoService;

    public InsumosController(IInsumoService insumoService)
    {
        _insumoService = insumoService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(InsumoViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CriarInsumoCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var insumo = await _insumoService.CriarAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = insumo.Id }, insumo);
        }
        catch (DomainException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (DbUpdateException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = MensagemErroInterno });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = MensagemErroInterno });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InsumoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new ObterInsumoPorIdQuery { Id = id };
            var insumo = await _insumoService.ObterPorIdAsync(query, cancellationToken);
            return Ok(insumo);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = MensagemErroInterno });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<InsumoViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            var insumos = await _insumoService.ListarAsync(new ListarInsumosQuery(), cancellationToken);
            return Ok(insumos);
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = MensagemErroInterno });
        }
    }

    [HttpPut]
    [ProducesResponseType(typeof(InsumoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromBody] AtualizarInsumoCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var insumo = await _insumoService.AtualizarAsync(command, cancellationToken);
            return Ok(insumo);
        }
        catch (DomainException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (DbUpdateException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = MensagemErroInterno });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = MensagemErroInterno });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ExcluirInsumoCommand { Id = id };
            await _insumoService.ExcluirAsync(command, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = MensagemErroInterno });
        }
    }
}
