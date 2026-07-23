using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Monolith.API.Features.OrdensServico;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.CreateOSService.API.Tests.Features.OrdensServico;

public sealed class OrdensServicoAdapterTests
{
    private readonly OrdensServicoAdapter _adapter = new();

    [Fact]
    public void Adapt_DeveRetornarCreatedAtRoute_QuandoAberturaOrdemServicoViewModelSucesso()
    {
        var abertura = new AberturaOrdemServicoViewModel { OrdemServicoId = Guid.NewGuid() };
        var result = new OrdensServicoResult<AberturaOrdemServicoViewModel, Exception>(abertura);

        var adaptado = _adapter.Adapt(result, true);

        var createdAtRoute = Assert.IsType<CreatedAtRoute<AberturaOrdemServicoViewModel>>(adaptado);
        Assert.Equal(abertura, createdAtRoute.Value);
    }

    [Fact]
    public void Adapt_DeveRetornarBadRequest_QuandoDomainException()
    {
        var exception = new DomainException("Dados inválidos");
        var result = new OrdensServicoResult<AberturaOrdemServicoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var statusCode = Assert.IsAssignableFrom<IStatusCodeHttpResult>(adaptado);
        Assert.Equal(StatusCodes.Status400BadRequest, statusCode.StatusCode);
    }

    [Fact]
    public void Adapt_DeveRetornarNotFound_QuandoKeyNotFoundException()
    {
        var exception = new KeyNotFoundException("Entidade não encontrada");
        var result = new OrdensServicoResult<AberturaOrdemServicoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var statusCode = Assert.IsAssignableFrom<IStatusCodeHttpResult>(adaptado);
        Assert.Equal(StatusCodes.Status404NotFound, statusCode.StatusCode);
    }

    [Fact]
    public void Adapt_DeveRetornarProblem_QuandoExceptionGenerica()
    {
        var exception = new InvalidOperationException("Erro inesperado");
        var result = new OrdensServicoResult<AberturaOrdemServicoViewModel, Exception>(exception);

        var adaptado = _adapter.Adapt(result);

        var problem = Assert.IsType<ProblemHttpResult>(adaptado);
        Assert.NotNull(problem);
    }
}
