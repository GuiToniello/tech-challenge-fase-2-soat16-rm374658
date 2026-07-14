using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using TechChallenge.Oficina.API.Features.Clientes;
using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.Queries;
using TechChallenge.Oficina.Application.Features.Clientes.Services;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.API.Tests.Features.Clientes;

public sealed class ClientEndpointsTests
{
    private readonly Mock<IClienteService> _serviceMock = new();

    [Fact]
    public async Task Post_DeveRetornarCreatedAtRoute_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var command = new CriarClienteCommand { NomeCompleto = "Cliente", Identificacao = "52998224725" };
        var cliente = new ClienteViewModel { Id = Guid.NewGuid(), NomeCompleto = "Cliente" };

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(resultado.Result);
        Assert.Equal("GetClienteById", created.RouteName);
        Assert.Equal(cliente, created.Value);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var endpoints = CriarEndpoints();
        var command = new CriarClienteCommand { NomeCompleto = "Cliente", Identificacao = "111" };

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("erro de domínio", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoClienteExiste()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();
        var cliente = new ClienteViewModel { Id = id, NomeCompleto = "Cliente" };

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterClientePorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var resultado = await endpoints.GetById(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<ClienteViewModel>>(resultado.Result);
        Assert.Equal(cliente, ok.Value);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoClienteNaoExiste()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterClientePorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var resultado = await endpoints.GetById(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("não encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Put_DeveRetornarOk_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var command = new AtualizarClienteCommand { Id = Guid.NewGuid(), NomeCompleto = "Atualizado", Identificacao = "52998224725" };
        var cliente = new ClienteViewModel { Id = command.Id, NomeCompleto = "Atualizado" };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var resultado = await endpoints.Put(command, CancellationToken.None);

        var ok = Assert.IsType<Ok<ClienteViewModel>>(resultado.Result);
        Assert.Equal(cliente, ok.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarBadRequest_QuandoDomainException()
    {
        var endpoints = CriarEndpoints();
        var command = new AtualizarClienteCommand { Id = Guid.NewGuid(), NomeCompleto = "Atualizado", Identificacao = "111" };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        var resultado = await endpoints.Put(command, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("erro de domínio", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task Put_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        var endpoints = CriarEndpoints();
        var command = new AtualizarClienteCommand { Id = Guid.NewGuid(), NomeCompleto = "Atualizado", Identificacao = "52998224725" };

        _serviceMock
            .Setup(service => service.AtualizarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var resultado = await endpoints.Put(command, CancellationToken.None);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("não encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Delete_DeveRetornarNoContent_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        var resultado = await endpoints.Delete(id, CancellationToken.None);

        Assert.IsType<NoContent>(resultado.Result);
        _serviceMock.Verify(service => service.ExcluirAsync(It.Is<ExcluirClienteCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.ExcluirAsync(It.Is<ExcluirClienteCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var resultado = await endpoints.Delete(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("não encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecao()
    {
        var endpoints = CriarEndpoints();
        IReadOnlyCollection<ClienteViewModel> clientes = [new ClienteViewModel { NomeCompleto = "Cliente" }];

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarClientesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        var resultado = await endpoints.Get(CancellationToken.None);

        Assert.Equal(clientes, resultado.Value);
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecaoVazia_QuandoSemClientes()
    {
        var endpoints = CriarEndpoints();
        IReadOnlyCollection<ClienteViewModel> clientes = [];

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarClientesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        var resultado = await endpoints.Get(CancellationToken.None);

        var value = Assert.IsAssignableFrom<IReadOnlyCollection<ClienteViewModel>>(resultado.Value);
        Assert.Empty(value);
    }

    private ClientEndpoints CriarEndpoints()
    {
        return new ClientEndpoints(_serviceMock.Object);
    }

    private static string? ObterMensagem(IReadOnlyDictionary<string, string?>? value)
    {
        return value is not null && value.TryGetValue("message", out var message) ? message : null;
    }
}
