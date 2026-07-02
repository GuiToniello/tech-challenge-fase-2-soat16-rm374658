using Microsoft.AspNetCore.Mvc;
using Moq;
using TechChallenge.Oficina.API.Features.Insumos;
using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.Queries;
using TechChallenge.Oficina.Application.Features.Insumos.Services;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.API.Tests.Features.Insumos;

public sealed class InsumosControllerTests
{
    private readonly Mock<IInsumoService> _serviceMock = new();

    private InsumosController CriarController() => new(_serviceMock.Object);

    [Fact]
    public async Task Post_DeveRetornarCreatedAtAction_QuandoSucesso()
    {
        var controller = CriarController();
        var command = new CriarInsumoCommand { Nome = "Óleo", Fabricante = "Bosch", QuantidadeDisponivel = 10, ValorUnitario = 19.9m };
        var model = new InsumoViewModel { Id = Guid.NewGuid(), Nome = "Óleo" };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var resultado = await controller.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(resultado);
        Assert.Equal(nameof(InsumosController.GetById), created.ActionName);
        Assert.Equal(model, created.Value);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var controller = CriarController();
        var command = new CriarInsumoCommand { Nome = "", Fabricante = "Bosch", QuantidadeDisponivel = 10, ValorUnitario = 19.9m };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ThrowsAsync(new DomainException("erro"));

        var resultado = await controller.Post(command, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("erro", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoNaoExiste()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.ObterPorIdAsync(It.Is<ObterInsumoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var resultado = await controller.GetById(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.Equal("não encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecao()
    {
        var controller = CriarController();
        IReadOnlyCollection<InsumoViewModel> insumos = [new InsumoViewModel { Nome = "Óleo" }];

        _serviceMock.Setup(s => s.ListarAsync(It.IsAny<ListarInsumosQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(insumos);

        var resultado = await controller.Get(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(insumos, ok.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        var controller = CriarController();
        var command = new AtualizarInsumoCommand { Id = Guid.NewGuid(), Nome = "Óleo", Fabricante = "Bosch", QuantidadeDisponivel = 10, ValorUnitario = 19.9m };

        _serviceMock.Setup(s => s.AtualizarAsync(command, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var resultado = await controller.Put(command, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.Equal("não encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Delete_DeveRetornarNoContent_QuandoSucesso()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();

        var resultado = await controller.Delete(id, CancellationToken.None);

        Assert.IsType<NoContentResult>(resultado);
        _serviceMock.Verify(s => s.ExcluirAsync(It.Is<ExcluirInsumoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static string? ObterMensagem(object? value)
    {
        return value?.GetType().GetProperty("message")?.GetValue(value)?.ToString();
    }
}
