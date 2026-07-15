using Moq;
using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.Queries;
using TechChallenge.Oficina.Application.Features.Clientes.Services;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Controllers.Features.Clientes;
using TechChallenge.Oficina.Domain.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Controllers.Tests.Features.Clientes;

public sealed class ClientEndpointsTests
{
    private readonly Mock<IClienteService> _serviceMock = new();
    private readonly Mock<IClientAdapter> _adapterMock = new();

    [Fact]
    public async Task Post_DeveRetornarCreatedAtRoute_QuandoSucesso()
    {
        var controller = CriarController();
        var command = new CriarClienteCommand { NomeCompleto = "Cliente", Identificacao = "52998224725" };
        var cliente = new ClienteViewModel { Id = Guid.NewGuid(), NomeCompleto = "Cliente" };
        var expectedResult = new ClienteResult<ClienteViewModel, Exception>(cliente);
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<ClienteResult<ClienteViewModel, Exception>>()))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<ClienteResult<ClienteViewModel, Exception>>()), Times.Once);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var controller = CriarController();
        var command = new CriarClienteCommand { NomeCompleto = "Cliente", Identificacao = "111" };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<ClienteResult<ClienteViewModel, Exception>>()))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<ClienteResult<ClienteViewModel, Exception>>()), Times.Once);
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

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(cliente, resultado.Value);
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

        Assert.Null(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<KeyNotFoundException>(resultado.Error);
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

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(cliente, resultado.Value);
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

        Assert.Null(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<DomainException>(resultado.Error);
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

        Assert.Null(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<KeyNotFoundException>(resultado.Error);
    }

    [Fact]
    public async Task Delete_DeveRetornarNoContent_QuandoSucesso()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();

        var resultado = await controller.Delete(id, CancellationToken.None);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.True(resultado.Value);
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

        Assert.False(resultado.Value);
        Assert.NotNull(resultado.Error);
        Assert.IsType<KeyNotFoundException>(resultado.Error);
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecao()
    {
        var controller = CriarController();
        IReadOnlyCollection<ClienteViewModel> clientes = new List<ClienteViewModel> { new ClienteViewModel { NomeCompleto = "Cliente" } };

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarClientesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        var resultado = await controller.Get(CancellationToken.None);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(clientes, resultado.Value);
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

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        var value = Assert.IsAssignableFrom<IReadOnlyCollection<ClienteViewModel>>(resultado.Value);
        Assert.Empty(value);
    }

    private ClienteController CriarController()
    {
        return new ClienteController(_serviceMock.Object, _adapterMock.Object);
    }
}
