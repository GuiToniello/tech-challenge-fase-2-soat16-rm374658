using Microsoft.AspNetCore.Mvc;
using Xunit;
using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Integration.Tests.Infrastructure;

namespace TechChallenge.Oficina.Integration.Tests.Features.Clientes;

public sealed class ClientesControllerIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;

    public ClientesControllerIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public void Dispose() => _fixture.Dispose();

    // ------------------------------------------------------------------ //
    // POST - Criar
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Post_ClienteValido_DeveRetornar201ComViewModel()
    {
        // Arrange
        var controller = _fixture.CriarClientesController();
        var command = new CriarClienteCommand
        {
            NomeCompleto = "João da Silva",
            Identificacao = "529.982.247-25",
            Email = "joao@email.com"
        };

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(resultado);
        var viewModel = Assert.IsType<ClienteViewModel>(created.Value);
        Assert.NotEqual(Guid.Empty, viewModel.Id);
        Assert.Equal("João da Silva", viewModel.NomeCompleto);
        Assert.Equal("joao@email.com", viewModel.Email);
    }

    [Fact]
    public async Task Post_CpfInvalido_DeveRetornar400()
    {
        // Arrange
        var controller = _fixture.CriarClientesController();
        var command = new CriarClienteCommand
        {
            NomeCompleto = "Maria Souza",
            Identificacao = "111.111.111-11"
        };

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task Post_NomeVazio_DeveRetornar400()
    {
        // Arrange
        var controller = _fixture.CriarClientesController();
        var command = new CriarClienteCommand
        {
            NomeCompleto = " ",
            Identificacao = "529.982.247-25"
        };

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task Post_IdentificacaoDuplicada_DeveRetornar400()
    {
        // Arrange
        var controller = _fixture.CriarClientesController();
        var cpf = "529.982.247-25";
        await controller.Post(new CriarClienteCommand { NomeCompleto = "Cliente Original", Identificacao = cpf }, CancellationToken.None);

        // Act
        var resultado = await controller.Post(new CriarClienteCommand { NomeCompleto = "Outro Cliente", Identificacao = cpf }, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    // ------------------------------------------------------------------ //
    // GET por id
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetById_ClienteExistente_DeveRetornar200ComViewModel()
    {
        // Arrange
        var controller = _fixture.CriarClientesController();
        var created = (CreatedAtActionResult)await controller.Post(
            new CriarClienteCommand { NomeCompleto = "Ana Lima", Identificacao = "529.982.247-25" },
            CancellationToken.None);
        var id = ((ClienteViewModel)created.Value!).Id;

        // Act
        var resultado = await controller.GetById(id, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var viewModel = Assert.IsType<ClienteViewModel>(ok.Value);
        Assert.Equal(id, viewModel.Id);
    }

    [Fact]
    public async Task GetById_ClienteInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarClientesController();

        // Act
        var resultado = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    // ------------------------------------------------------------------ //
    // GET lista
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Get_SemClientes_DeveRetornar200ComListaVazia()
    {
        // Arrange
        var controller = _fixture.CriarClientesController();

        // Act
        var resultado = await controller.Get(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var lista = Assert.IsAssignableFrom<IEnumerable<ClienteViewModel>>(ok.Value);
        Assert.Empty(lista);
    }

    [Fact]
    public async Task Get_ComClientes_DeveRetornar200ComLista()
    {
        // Arrange
        var controller = _fixture.CriarClientesController();
        await controller.Post(new CriarClienteCommand { NomeCompleto = "Pedro Costa", Identificacao = "529.982.247-25" }, CancellationToken.None);

        // Act
        var resultado = await controller.Get(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var lista = Assert.IsAssignableFrom<IEnumerable<ClienteViewModel>>(ok.Value);
        Assert.NotEmpty(lista);
    }

    // ------------------------------------------------------------------ //
    // PUT - Atualizar
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Put_ClienteExistente_DeveRetornar200ComDadosAtualizados()
    {
        // Arrange
        var controller = _fixture.CriarClientesController();
        var created = (CreatedAtActionResult)await controller.Post(
            new CriarClienteCommand { NomeCompleto = "Fernanda Rocha", Identificacao = "529.982.247-25" },
            CancellationToken.None);
        var id = ((ClienteViewModel)created.Value!).Id;
        var command = new AtualizarClienteCommand
        {
            Id = id,
            NomeCompleto = "Fernanda Rocha Atualizada",
            Identificacao = "529.982.247-25"
        };

        // Act
        var resultado = await controller.Put(command, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var viewModel = Assert.IsType<ClienteViewModel>(ok.Value);
        Assert.Equal("Fernanda Rocha Atualizada", viewModel.NomeCompleto);
    }

    [Fact]
    public async Task Put_ClienteInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarClientesController();
        var command = new AtualizarClienteCommand
        {
            Id = Guid.NewGuid(),
            NomeCompleto = "Nome Qualquer",
            Identificacao = "529.982.247-25"
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
    public async Task Delete_ClienteExistente_DeveRetornar204()
    {
        // Arrange
        var controller = _fixture.CriarClientesController();
        var created = (CreatedAtActionResult)await controller.Post(
            new CriarClienteCommand { NomeCompleto = "Lucas Martins", Identificacao = "529.982.247-25" },
            CancellationToken.None);
        var id = ((ClienteViewModel)created.Value!).Id;

        // Act
        var resultado = await controller.Delete(id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(resultado);
    }

    [Fact]
    public async Task Delete_ClienteInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarClientesController();

        // Act
        var resultado = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }
}
