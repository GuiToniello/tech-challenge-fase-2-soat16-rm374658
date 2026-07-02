using Microsoft.AspNetCore.Mvc;
using TechChallenge.Oficina.Application.Features.OrdensServico.Commands;
using TechChallenge.Oficina.Application.Features.OrdensServico.Queries;
using TechChallenge.Oficina.Application.Features.OrdensServico.Services;
using TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;

namespace TechChallenge.Oficina.API.Features.OrdensServico;

[ApiController]
[Route("api/ordens-servico")]
public sealed class OrdensServicoController : ControllerBase
{
    private readonly IOrdemServicoService _ordemServicoService;

    public OrdensServicoController(IOrdemServicoService ordemServicoService)
    {
        _ordemServicoService = ordemServicoService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrdemServicoViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post([FromBody] CriarOrdemServicoCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var ordemServico = await _ordemServicoService.CriarAsync(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = ordemServico.Id }, ordemServico);
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
    [ProducesResponseType(typeof(OrdemServicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ordemServico = await _ordemServicoService.ObterPorIdAsync(new ObterOrdemServicoPorIdQuery { Id = id }, cancellationToken);
            return Ok(ordemServico);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrdemServicoViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var ordensServico = await _ordemServicoService.ListarAsync(new ListarOrdensServicoQuery(), cancellationToken);
        return Ok(ordensServico);
    }

    [HttpGet("{id:guid}/acompanhamento")]
    [ProducesResponseType(typeof(AcompanhamentoOrdemServicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAcompanhamento([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var acompanhamento = await _ordemServicoService.ObterAcompanhamentoAsync(new ObterAcompanhamentoOrdemServicoPorIdQuery { Id = id }, cancellationToken);
            return Ok(acompanhamento);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpGet("cliente/{clienteId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<AcompanhamentoOrdemServicoViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCliente([FromRoute] Guid clienteId, CancellationToken cancellationToken)
    {
        try
        {
            var acompanhamentos = await _ordemServicoService.ListarPorClienteAsync(new ListarOrdensServicoPorClienteQuery { ClienteId = clienteId }, cancellationToken);
            return Ok(acompanhamentos);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPut]
    [ProducesResponseType(typeof(OrdemServicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put([FromBody] AtualizarOrdemServicoCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var ordemServico = await _ordemServicoService.AtualizarAsync(command, cancellationToken);
            return Ok(ordemServico);
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
            await _ordemServicoService.ExcluirAsync(new ExcluirOrdemServicoCommand { Id = id }, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPost("{id:guid}/em-diagnostico")]
    [ProducesResponseType(typeof(OrdemServicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarParaEmDiagnostico([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ordemServico = await _ordemServicoService.AlterarStatusParaEmDiagnosticoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
            return Ok(ordemServico);
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

    [HttpPost("{id:guid}/em-execucao")]
    [ProducesResponseType(typeof(OrdemServicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarParaEmExecucao([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ordemServico = await _ordemServicoService.AlterarStatusParaEmExecucaoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
            return Ok(ordemServico);
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

    [HttpPost("{id:guid}/finalizar")]
    [ProducesResponseType(typeof(OrdemServicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarParaFinalizada([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ordemServico = await _ordemServicoService.AlterarStatusParaFinalizadaAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
            return Ok(ordemServico);
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

    [HttpPost("{id:guid}/entregar")]
    [ProducesResponseType(typeof(OrdemServicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarParaEntregue([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ordemServico = await _ordemServicoService.AlterarStatusParaEntregueAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
            return Ok(ordemServico);
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

    [HttpPost("{id:guid}/gerar-orcamento")]
    [ProducesResponseType(typeof(OrdemServicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GerarOrcamento([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ordemServico = await _ordemServicoService.GerarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
            return Ok(ordemServico);
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

    [HttpPost("{id:guid}/enviar-orcamento")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnviarOrcamento([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _ordemServicoService.EnviarOrcamentoPorEmailAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
            return NoContent();
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

    [HttpPost("{id:guid}/aprovar-orcamento")]
    [ProducesResponseType(typeof(OrdemServicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AprovarOrcamento([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ordemServico = await _ordemServicoService.AprovarOrcamentoAsync(new AlterarStatusOrdemServicoCommand { Id = id }, cancellationToken);
            return Ok(ordemServico);
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
}
