using Microsoft.AspNetCore.Mvc;
using Xunit;
using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Integration.Tests.Infrastructure;

namespace TechChallenge.Oficina.Integration.Tests.Features.Insumos;

public sealed class InsumosControllerIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;

    public InsumosControllerIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public void Dispose() => _fixture.Dispose();

    private static CriarInsumoCommand InsumoValido(string nome = "Óleo Lubrificante") => new()
    {
        Nome = nome,
        Fabricante = "Mobil",
        QuantidadeDisponivel = 100,
        ValorUnitario = 49.90m
    };

    // ------------------------------------------------------------------ //
    // POST - Criar
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Post_InsumoValido_DeveRetornar201ComViewModel()
    {
        // Arrange
        var controller = _fixture.CriarInsumosController();
        var command = InsumoValido();

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(resultado);
        var viewModel = Assert.IsType<InsumoViewModel>(created.Value);
        Assert.NotEqual(Guid.Empty, viewModel.Id);
        Assert.Equal("Óleo Lubrificante", viewModel.Nome);
        Assert.Equal(49.90m, viewModel.ValorUnitario);
    }

    [Fact]
    public async Task Post_NomeVazio_DeveRetornar400()
    {
        // Arrange
        var controller = _fixture.CriarInsumosController();
        var command = new CriarInsumoCommand { Nome = " ", Fabricante = "Mobil", QuantidadeDisponivel = 10, ValorUnitario = 10 };

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task Post_ValorNegativo_DeveRetornar400()
    {
        // Arrange
        var controller = _fixture.CriarInsumosController();
        var command = new CriarInsumoCommand { Nome = "Filtro de Ar", Fabricante = "Mann", QuantidadeDisponivel = 10, ValorUnitario = -5m };

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    // ------------------------------------------------------------------ //
    // GET por id
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetById_InsumoExistente_DeveRetornar200ComViewModel()
    {
        // Arrange
        var controller = _fixture.CriarInsumosController();
        var created = (CreatedAtActionResult)await controller.Post(InsumoValido(), CancellationToken.None);
        var id = ((InsumoViewModel)created.Value!).Id;

        // Act
        var resultado = await controller.GetById(id, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var viewModel = Assert.IsType<InsumoViewModel>(ok.Value);
        Assert.Equal(id, viewModel.Id);
    }

    [Fact]
    public async Task GetById_InsumoInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarInsumosController();

        // Act
        var resultado = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    // ------------------------------------------------------------------ //
    // GET lista
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Get_ComInsumos_DeveRetornar200ComLista()
    {
        // Arrange
        var controller = _fixture.CriarInsumosController();
        await controller.Post(InsumoValido("Vela de Ignição"), CancellationToken.None);

        // Act
        var resultado = await controller.Get(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var lista = Assert.IsAssignableFrom<IEnumerable<InsumoViewModel>>(ok.Value);
        Assert.NotEmpty(lista);
    }

    // ------------------------------------------------------------------ //
    // PUT - Atualizar
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Put_InsumoExistente_DeveRetornar200ComDadosAtualizados()
    {
        // Arrange
        var controller = _fixture.CriarInsumosController();
        var created = (CreatedAtActionResult)await controller.Post(InsumoValido(), CancellationToken.None);
        var id = ((InsumoViewModel)created.Value!).Id;
        var command = new AtualizarInsumoCommand
        {
            Id = id,
            Nome = "Óleo Lubrificante Premium",
            Fabricante = "Castrol",
            QuantidadeDisponivel = 50,
            ValorUnitario = 79.90m
        };

        // Act
        var resultado = await controller.Put(command, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var viewModel = Assert.IsType<InsumoViewModel>(ok.Value);
        Assert.Equal("Óleo Lubrificante Premium", viewModel.Nome);
        Assert.Equal(79.90m, viewModel.ValorUnitario);
    }

    [Fact]
    public async Task Put_InsumoInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarInsumosController();
        var command = new AtualizarInsumoCommand
        {
            Id = Guid.NewGuid(),
            Nome = "Qualquer",
            Fabricante = "Fab",
            QuantidadeDisponivel = 1,
            ValorUnitario = 1m
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
    public async Task Delete_InsumoExistente_DeveRetornar204()
    {
        // Arrange
        var controller = _fixture.CriarInsumosController();
        var created = (CreatedAtActionResult)await controller.Post(InsumoValido(), CancellationToken.None);
        var id = ((InsumoViewModel)created.Value!).Id;

        // Act
        var resultado = await controller.Delete(id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(resultado);
    }

    [Fact]
    public async Task Delete_InsumoInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarInsumosController();

        // Act
        var resultado = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }
}
