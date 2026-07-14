using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using TechChallenge.Oficina.API.Features.Veiculos;
using TechChallenge.Oficina.Application.Features.Veiculos.Commands;
using TechChallenge.Oficina.Application.Features.Veiculos.Queries;
using TechChallenge.Oficina.Application.Features.Veiculos.Services;
using TechChallenge.Oficina.Application.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.API.Tests.Features.Veiculos;

public sealed class VeiculosControllerTests
{
    private readonly Mock<IVeiculoService> _serviceMock = new();

    private VeiculoEndpoints CriarEndpoints() => new(_serviceMock.Object);

    private static string? ObterMensagem(Dictionary<string, string?>? value)
    {
        return value?["message"];
    }

    [Fact]
    public async Task Post_DeveRetornarCreatedAtRoute_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var command = new CriarVeiculoCommand { Placa = "ABC1D23", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };
        var veiculo = new VeiculoViewModel { Id = Guid.NewGuid(), Placa = "ABC1D23" };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtRoute<VeiculoViewModel>>(resultado.Result);
        Assert.Equal("GetVeiculoById", created.RouteName);
        Assert.Equal(veiculo, created.Value);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var endpoints = CriarEndpoints();
        var command = new CriarVeiculoCommand { Placa = "INVALIDA", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ThrowsAsync(new DomainException("placa inválida"));

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("placa inválida", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task Post_DeveRetornarNotFound_QuandoClienteNaoExiste()
    {
        var endpoints = CriarEndpoints();
        var command = new CriarVeiculoCommand { Placa = "ABC1D23", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("Cliente não encontrado."));

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("Cliente não encontrado.", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoVeiculoExiste()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();
        var veiculo = new VeiculoViewModel { Id = id, Placa = "ABC1D23" };

        _serviceMock.Setup(s => s.ObterPorIdAsync(It.Is<ObterVeiculoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        var resultado = await endpoints.GetById(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<VeiculoViewModel>>(resultado.Result);
        Assert.Equal(veiculo, ok.Value);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoVeiculoNaoExiste()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.ObterPorIdAsync(It.Is<ObterVeiculoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var resultado = await endpoints.GetById(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("não encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Get_DeveRetornarOkComLista()
    {
        var endpoints = CriarEndpoints();
        var veiculos = new[] { new VeiculoViewModel { Id = Guid.NewGuid(), Placa = "ABC1D23" } };

        _serviceMock.Setup(s => s.ListarAsync(It.IsAny<ListarVeiculosQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(veiculos);

        var resultado = await endpoints.Get(null, CancellationToken.None);

        var ok = Assert.IsType<Ok<IReadOnlyCollection<VeiculoViewModel>>>(resultado);
        Assert.Equal(veiculos, ok.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarOk_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var command = new AtualizarVeiculoCommand { Id = Guid.NewGuid(), Placa = "ABC1D23", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };
        var veiculo = new VeiculoViewModel { Id = command.Id, Placa = "ABC1D23" };

        _serviceMock.Setup(s => s.AtualizarAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        var resultado = await endpoints.Put(command, CancellationToken.None);

        var ok = Assert.IsType<Ok<VeiculoViewModel>>(resultado.Result);
        Assert.Equal(veiculo, ok.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarBadRequest_QuandoDomainException()
    {
        var endpoints = CriarEndpoints();
        var command = new AtualizarVeiculoCommand { Id = Guid.NewGuid(), Placa = "INVALIDA", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };

        _serviceMock.Setup(s => s.AtualizarAsync(command, It.IsAny<CancellationToken>())).ThrowsAsync(new DomainException("placa inválida"));

        var resultado = await endpoints.Put(command, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("placa inválida", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task Put_DeveRetornarNotFound_QuandoKeyNotFoundException()
    {
        var endpoints = CriarEndpoints();
        var command = new AtualizarVeiculoCommand { Id = Guid.NewGuid(), Placa = "ABC1D23", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };

        _serviceMock.Setup(s => s.AtualizarAsync(command, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var resultado = await endpoints.Put(command, CancellationToken.None);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("não encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Delete_DeveRetornarNoContent_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.ExcluirAsync(It.Is<ExcluirVeiculoCommand>(c => c.Id == id), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var resultado = await endpoints.Delete(id, CancellationToken.None);

        Assert.IsType<NoContent>(resultado.Result);
    }

    [Fact]
    public async Task Delete_DeveRetornarNotFound_QuandoNaoExiste()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.ExcluirAsync(It.Is<ExcluirVeiculoCommand>(c => c.Id == id), It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var resultado = await endpoints.Delete(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("não encontrado", ObterMensagem(notFound.Value));
    }
}
