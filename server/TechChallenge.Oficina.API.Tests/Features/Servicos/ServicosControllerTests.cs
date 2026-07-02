using Microsoft.AspNetCore.Mvc;
using Moq;
using TechChallenge.Oficina.API.Features.Servicos;
using TechChallenge.Oficina.Application.Features.Servicos.Commands;
using TechChallenge.Oficina.Application.Features.Servicos.Queries;
using TechChallenge.Oficina.Application.Features.Servicos.Services;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.API.Tests.Features.Servicos;

public sealed class ServicosControllerTests
{
    private readonly Mock<IServicoService> _serviceMock = new();

    private ServicosController CriarController() => new(_serviceMock.Object);

    [Fact]
    public async Task Post_DeveRetornarCreatedAtAction_QuandoSucesso()
    {
        var controller = CriarController();
        var command = new CriarServicoCommand { Nome = "Troca", Descricao = "Descricao", ItensServico = [] };
        var model = new ServicoViewModel { Id = Guid.NewGuid(), Nome = "Troca" };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var resultado = await controller.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(resultado);
        Assert.Equal(nameof(ServicosController.GetById), created.ActionName);
        Assert.Equal(model, created.Value);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var controller = CriarController();
        var command = new CriarServicoCommand { Nome = "", Descricao = "Descricao", ItensServico = [] };

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

        _serviceMock.Setup(s => s.ObterPorIdAsync(It.Is<ObterServicoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("nao encontrado"));

        var resultado = await controller.GetById(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.Equal("nao encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecao()
    {
        var controller = CriarController();
        IReadOnlyCollection<ServicoViewModel> servicos = [new ServicoViewModel { Nome = "Troca" }];

        _serviceMock.Setup(s => s.ListarAsync(It.IsAny<ListarServicosQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(servicos);

        var resultado = await controller.Get(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(servicos, ok.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        var controller = CriarController();
        var command = new AtualizarServicoCommand { Id = Guid.NewGuid(), Nome = "Troca", Descricao = "Descricao", ItensServico = [] };

        _serviceMock.Setup(s => s.AtualizarAsync(command, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("nao encontrado"));

        var resultado = await controller.Put(command, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.Equal("nao encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Delete_DeveRetornarNoContent_QuandoSucesso()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();

        var resultado = await controller.Delete(id, CancellationToken.None);

        Assert.IsType<NoContentResult>(resultado);
        _serviceMock.Verify(s => s.ExcluirAsync(It.Is<ExcluirServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static string? ObterMensagem(object? value)
    {
        return value?.GetType().GetProperty("message")?.GetValue(value)?.ToString();
    }
}
