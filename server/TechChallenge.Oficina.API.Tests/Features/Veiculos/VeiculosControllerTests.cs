using Microsoft.AspNetCore.Mvc;
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

    private VeiculosController CriarController() => new(_serviceMock.Object);

    private static string? ObterMensagem(object? value)
    {
        return value?.GetType().GetProperty("message")?.GetValue(value)?.ToString();
    }

    [Fact]
    public async Task Post_DeveRetornarCreatedAtAction_QuandoSucesso()
    {
        var controller = CriarController();
        var command = new CriarVeiculoCommand { Placa = "ABC1D23", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };
        var veiculo = new VeiculoViewModel { Id = Guid.NewGuid(), Placa = "ABC1D23" };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        var resultado = await controller.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(resultado);
        Assert.Equal(nameof(VeiculosController.GetById), created.ActionName);
        Assert.Equal(veiculo, created.Value);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var controller = CriarController();
        var command = new CriarVeiculoCommand { Placa = "INVALIDA", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ThrowsAsync(new DomainException("placa inválida"));

        var resultado = await controller.Post(command, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("placa inválida", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task Post_DeveRetornarNotFound_QuandoClienteNaoExiste()
    {
        var controller = CriarController();
        var command = new CriarVeiculoCommand { Placa = "ABC1D23", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("Cliente não encontrado."));

        var resultado = await controller.Post(command, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.Equal("Cliente não encontrado.", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task GetById_DeveRetornarOk_QuandoVeiculoExiste()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();
        var veiculo = new VeiculoViewModel { Id = id, Placa = "ABC1D23" };

        _serviceMock.Setup(s => s.ObterPorIdAsync(It.Is<ObterVeiculoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        var resultado = await controller.GetById(id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(veiculo, ok.Value);
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoVeiculoNaoExiste()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.ObterPorIdAsync(It.Is<ObterVeiculoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var resultado = await controller.GetById(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.Equal("não encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Get_DeveRetornarOkComLista()
    {
        var controller = CriarController();
        var veiculos = new[] { new VeiculoViewModel { Id = Guid.NewGuid(), Placa = "ABC1D23" } };

        _serviceMock.Setup(s => s.ListarAsync(It.IsAny<ListarVeiculosQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(veiculos);

        var resultado = await controller.Get(null, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(veiculos, ok.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarOk_QuandoSucesso()
    {
        var controller = CriarController();
        var command = new AtualizarVeiculoCommand { Id = Guid.NewGuid(), Placa = "ABC1D23", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };
        var veiculo = new VeiculoViewModel { Id = command.Id, Placa = "ABC1D23" };

        _serviceMock.Setup(s => s.AtualizarAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);

        var resultado = await controller.Put(command, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(veiculo, ok.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarBadRequest_QuandoDomainException()
    {
        var controller = CriarController();
        var command = new AtualizarVeiculoCommand { Id = Guid.NewGuid(), Placa = "INVALIDA", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };

        _serviceMock.Setup(s => s.AtualizarAsync(command, It.IsAny<CancellationToken>())).ThrowsAsync(new DomainException("placa inválida"));

        var resultado = await controller.Put(command, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal("placa inválida", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task Put_DeveRetornarNotFound_QuandoKeyNotFoundException()
    {
        var controller = CriarController();
        var command = new AtualizarVeiculoCommand { Id = Guid.NewGuid(), Placa = "ABC1D23", Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Renavam = "12345678901", ClienteId = Guid.NewGuid() };

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

        _serviceMock.Setup(s => s.ExcluirAsync(It.Is<ExcluirVeiculoCommand>(c => c.Id == id), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var resultado = await controller.Delete(id, CancellationToken.None);

        Assert.IsType<NoContentResult>(resultado);
    }

    [Fact]
    public async Task Delete_DeveRetornarNotFound_QuandoNaoExiste()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.ExcluirAsync(It.Is<ExcluirVeiculoCommand>(c => c.Id == id), It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var resultado = await controller.Delete(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
        Assert.Equal("não encontrado", ObterMensagem(notFound.Value));
    }
}
