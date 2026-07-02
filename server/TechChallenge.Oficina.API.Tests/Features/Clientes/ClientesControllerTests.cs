using Microsoft.AspNetCore.Mvc;
using Moq;
using TechChallenge.Oficina.API.Features.Clientes;
using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.Queries;
using TechChallenge.Oficina.Application.Features.Clientes.Services;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.API.Tests.Features.Clientes;

public sealed class ClientesControllerTests
{
    private readonly Mock<IClienteService> _serviceMock = new();

    [Fact]
    public async Task Post_DeveRetornarCreatedAtAction_QuandoSucesso()
    {
        var controller = CriarController();
        var command = new CriarClienteCommand { NomeCompleto = "Cliente", Identificacao = "52998224725" };
        var cliente = new ClienteViewModel { Id = Guid.NewGuid(), NomeCompleto = "Cliente" };

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var resultado = await controller.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(resultado);
        Assert.Equal(nameof(ClientesController.GetById), created.ActionName);
        Assert.Equal(cliente, created.Value);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var controller = CriarController();
        var command = new CriarClienteCommand { NomeCompleto = "Cliente", Identificacao = "111" };

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        var resultado = await controller.Post(command, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("erro de domínio", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoClienteExiste()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();
        var cliente = new ClienteViewModel { Id = id, NomeCompleto = "Cliente" };

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterClientePorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var resultado = await controller.GetById(id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(cliente, ok.Value);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoClienteNaoExiste()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterClientePorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var resultado = await controller.GetById(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.Equal("não encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Put_DeveRetornarOk_QuandoSucesso()
    {
        var controller = CriarController();
        var command = new AtualizarClienteCommand { Id = Guid.NewGuid(), NomeCompleto = "Atualizado", Identificacao = "52998224725" };
        var cliente = new ClienteViewModel { Id = command.Id, NomeCompleto = "Atualizado" };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var resultado = await controller.Put(command, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(cliente, ok.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarBadRequest_QuandoDomainException()
    {
        var controller = CriarController();
        var command = new AtualizarClienteCommand { Id = Guid.NewGuid(), NomeCompleto = "Atualizado", Identificacao = "111" };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        var resultado = await controller.Put(command, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("erro de domínio", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task Put_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        var controller = CriarController();
        var command = new AtualizarClienteCommand { Id = Guid.NewGuid(), NomeCompleto = "Atualizado", Identificacao = "52998224725" };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

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
        _serviceMock.Verify(service => service.ExcluirAsync(It.Is<ExcluirClienteCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.ExcluirAsync(It.Is<ExcluirClienteCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var resultado = await controller.Delete(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.Equal("não encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecao()
    {
        var controller = CriarController();
        IReadOnlyCollection<ClienteViewModel> clientes = [new ClienteViewModel { NomeCompleto = "Cliente" }];

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarClientesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        var resultado = await controller.Get(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(clientes, ok.Value);
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecaoVazia_QuandoSemClientes()
    {
        var controller = CriarController();
        IReadOnlyCollection<ClienteViewModel> clientes = [];

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarClientesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        var resultado = await controller.Get(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        var value = Assert.IsAssignableFrom<IReadOnlyCollection<ClienteViewModel>>(ok.Value);
        Assert.Empty(value);
    }

    private ClientesController CriarController()
    {
        return new ClientesController(_serviceMock.Object);
    }

    private static string? ObterMensagem(object? value)
    {
        return value?.GetType().GetProperty("message")?.GetValue(value)?.ToString();
    }
}
