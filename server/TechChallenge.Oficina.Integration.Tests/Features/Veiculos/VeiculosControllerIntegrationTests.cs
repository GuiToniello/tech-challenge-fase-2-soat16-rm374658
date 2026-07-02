using Microsoft.AspNetCore.Mvc;
using Xunit;
using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Application.Features.Veiculos.Commands;
using TechChallenge.Oficina.Application.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Integration.Tests.Infrastructure;

namespace TechChallenge.Oficina.Integration.Tests.Features.Veiculos;

public sealed class VeiculosControllerIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;

    public VeiculosControllerIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public void Dispose() => _fixture.Dispose();

    private async Task<Guid> CriarClienteAsync()
    {
        var clienteController = _fixture.CriarClientesController();
        var result = (CreatedAtActionResult)await clienteController.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente Teste", Identificacao = "529.982.247-25" },
            CancellationToken.None);
        return ((ClienteViewModel)result.Value!).Id;
    }

    private static CriarVeiculoCommand VeiculoValido(Guid clienteId) => new()
    {
        Placa = "ABC1D23",
        Marca = "Toyota",
        Modelo = "Corolla",
        Ano = 2023,
        Renavam = "12345678901",
        ClienteId = clienteId
    };

    // ------------------------------------------------------------------ //
    // POST - Criar
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Post_VeiculoValido_DeveRetornar201ComViewModel()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var controller = _fixture.CriarVeiculosController();
        var command = VeiculoValido(clienteId);

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(resultado);
        var viewModel = Assert.IsType<VeiculoViewModel>(created.Value);
        Assert.NotEqual(Guid.Empty, viewModel.Id);
        Assert.Equal("ABC1D23", viewModel.Placa);
        Assert.Equal(clienteId, viewModel.ClienteId);
    }

    [Fact]
    public async Task Post_ClienteInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarVeiculosController();
        var command = VeiculoValido(Guid.NewGuid());

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task Post_PlacaInvalida_DeveRetornar400()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var controller = _fixture.CriarVeiculosController();
        var command = new CriarVeiculoCommand
        {
            Placa = "INVALIDA",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023,
            Renavam = "12345678901",
            ClienteId = clienteId
        };

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task Post_PlacaDuplicada_DeveRetornar400()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var controller = _fixture.CriarVeiculosController();
        await controller.Post(VeiculoValido(clienteId), CancellationToken.None);

        // Act
        var resultado = await controller.Post(VeiculoValido(clienteId), CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    // ------------------------------------------------------------------ //
    // GET por id
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetById_VeiculoExistente_DeveRetornar200ComViewModel()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var controller = _fixture.CriarVeiculosController();
        var created = (CreatedAtActionResult)await controller.Post(VeiculoValido(clienteId), CancellationToken.None);
        var id = ((VeiculoViewModel)created.Value!).Id;

        // Act
        var resultado = await controller.GetById(id, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var viewModel = Assert.IsType<VeiculoViewModel>(ok.Value);
        Assert.Equal(id, viewModel.Id);
    }

    [Fact]
    public async Task GetById_VeiculoInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarVeiculosController();

        // Act
        var resultado = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    // ------------------------------------------------------------------ //
    // GET lista por clienteId
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Get_FiltrandoPorClienteId_DeveRetornar200SomenteVeiculosDoCliente()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var controller = _fixture.CriarVeiculosController();
        await controller.Post(VeiculoValido(clienteId), CancellationToken.None);

        // Act
        var resultado = await controller.Get(clienteId, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var lista = Assert.IsAssignableFrom<IEnumerable<VeiculoViewModel>>(ok.Value);
        Assert.All(lista, v => Assert.Equal(clienteId, v.ClienteId));
    }

    // ------------------------------------------------------------------ //
    // PUT - Atualizar
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Put_VeiculoExistente_DeveRetornar200ComDadosAtualizados()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var controller = _fixture.CriarVeiculosController();
        var created = (CreatedAtActionResult)await controller.Post(VeiculoValido(clienteId), CancellationToken.None);
        var id = ((VeiculoViewModel)created.Value!).Id;
        var command = new AtualizarVeiculoCommand
        {
            Id = id,
            Placa = "XYZ9W88",
            Marca = "Honda",
            Modelo = "Civic",
            Ano = 2024,
            Renavam = "12345678901",
            ClienteId = clienteId
        };

        // Act
        var resultado = await controller.Put(command, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var viewModel = Assert.IsType<VeiculoViewModel>(ok.Value);
        Assert.Equal("XYZ9W88", viewModel.Placa);
        Assert.Equal("Honda", viewModel.Marca);
    }

    [Fact]
    public async Task Put_VeiculoInexistente_DeveRetornar404()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var controller = _fixture.CriarVeiculosController();
        var command = new AtualizarVeiculoCommand
        {
            Id = Guid.NewGuid(),
            Placa = "ABC1D23",
            Marca = "Toyota",
            Modelo = "Corolla",
            Ano = 2023,
            Renavam = "12345678901",
            ClienteId = clienteId
        };

        // Act
        var resultado = await controller.Put(command, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    // ------------------------------------------------------------------ //
    // DELETE
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Delete_VeiculoExistente_DeveRetornar204()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var controller = _fixture.CriarVeiculosController();
        var created = (CreatedAtActionResult)await controller.Post(VeiculoValido(clienteId), CancellationToken.None);
        var id = ((VeiculoViewModel)created.Value!).Id;

        // Act
        var resultado = await controller.Delete(id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(resultado);
    }

    [Fact]
    public async Task Delete_VeiculoInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarVeiculosController();

        // Act
        var resultado = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }
}
