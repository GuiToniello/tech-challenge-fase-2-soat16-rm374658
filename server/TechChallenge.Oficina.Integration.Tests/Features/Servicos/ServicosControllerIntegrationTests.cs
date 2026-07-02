using Microsoft.AspNetCore.Mvc;
using Xunit;
using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Application.Features.Servicos.Commands;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Integration.Tests.Infrastructure;

namespace TechChallenge.Oficina.Integration.Tests.Features.Servicos;

public sealed class ServicosControllerIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;

    public ServicosControllerIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public void Dispose() => _fixture.Dispose();

    private async Task<Guid> CriarInsumoAsync(string nome = "Filtro de Ar")
    {
        var controller = _fixture.CriarInsumosController();
        var result = (CreatedAtActionResult)await controller.Post(
            new CriarInsumoCommand { Nome = nome, Fabricante = "Mann", QuantidadeDisponivel = 50, ValorUnitario = 25m },
            CancellationToken.None);
        return ((InsumoViewModel)result.Value!).Id;
    }

    private static CriarServicoCommand ServicoSemItens(string nome = "Revisão Geral") => new()
    {
        Nome = nome,
        Descricao = "Revisão completa do veículo",
        ItensServico = []
    };

    // ------------------------------------------------------------------ //
    // POST - Criar
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Post_ServicoValido_DeveRetornar201ComViewModel()
    {
        // Arrange
        var controller = _fixture.CriarServicosController();
        var command = ServicoSemItens();

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(resultado);
        var viewModel = Assert.IsType<ServicoViewModel>(created.Value);
        Assert.NotEqual(Guid.Empty, viewModel.Id);
        Assert.Equal("Revisão Geral", viewModel.Nome);
    }

    [Fact]
    public async Task Post_ServicoComItens_DeveRetornar201ComItensAssociados()
    {
        // Arrange
        var insumoId = await CriarInsumoAsync();
        var controller = _fixture.CriarServicosController();
        var command = new CriarServicoCommand
        {
            Nome = "Troca de Filtro",
            Descricao = "Substituição do filtro de ar",
            ItensServico = [new ItemServicoCommand { InsumoId = insumoId, Quantidade = 1 }]
        };

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(resultado);
        var viewModel = Assert.IsType<ServicoViewModel>(created.Value);
        Assert.Single(viewModel.ItensServico);
    }

    [Fact]
    public async Task Post_NomeVazio_DeveRetornar400()
    {
        // Arrange
        var controller = _fixture.CriarServicosController();
        var command = new CriarServicoCommand { Nome = " ", Descricao = "Descrição válida", ItensServico = [] };

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task Post_InsumoInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarServicosController();
        var command = new CriarServicoCommand
        {
            Nome = "Serviço X",
            Descricao = "Desc X",
            ItensServico = [new ItemServicoCommand { InsumoId = Guid.NewGuid(), Quantidade = 1 }]
        };

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    // ------------------------------------------------------------------ //
    // GET por id
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetById_ServicoExistente_DeveRetornar200ComViewModel()
    {
        // Arrange
        var controller = _fixture.CriarServicosController();
        var created = (CreatedAtActionResult)await controller.Post(ServicoSemItens(), CancellationToken.None);
        var id = ((ServicoViewModel)created.Value!).Id;

        // Act
        var resultado = await controller.GetById(id, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var viewModel = Assert.IsType<ServicoViewModel>(ok.Value);
        Assert.Equal(id, viewModel.Id);
    }

    [Fact]
    public async Task GetById_ServicoInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarServicosController();

        // Act
        var resultado = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    // ------------------------------------------------------------------ //
    // GET lista
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Get_ComServicos_DeveRetornar200ComLista()
    {
        // Arrange
        var controller = _fixture.CriarServicosController();
        await controller.Post(ServicoSemItens("Alinhamento"), CancellationToken.None);

        // Act
        var resultado = await controller.Get(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var lista = Assert.IsAssignableFrom<IEnumerable<ServicoViewModel>>(ok.Value);
        Assert.NotEmpty(lista);
    }

    // ------------------------------------------------------------------ //
    // PUT - Atualizar
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Put_ServicoExistente_DeveRetornar200ComDadosAtualizados()
    {
        // Arrange
        var controller = _fixture.CriarServicosController();
        var created = (CreatedAtActionResult)await controller.Post(ServicoSemItens(), CancellationToken.None);
        var id = ((ServicoViewModel)created.Value!).Id;
        var command = new AtualizarServicoCommand
        {
            Id = id,
            Nome = "Revisão Geral Atualizada",
            Descricao = "Revisão completa atualizada",
            ItensServico = []
        };

        // Act
        var resultado = await controller.Put(command, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var viewModel = Assert.IsType<ServicoViewModel>(ok.Value);
        Assert.Equal("Revisão Geral Atualizada", viewModel.Nome);
    }

    [Fact]
    public async Task Put_ServicoInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarServicosController();
        var command = new AtualizarServicoCommand
        {
            Id = Guid.NewGuid(),
            Nome = "Qualquer",
            Descricao = "Desc",
            ItensServico = []
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
    public async Task Delete_ServicoExistente_DeveRetornar204()
    {
        // Arrange
        var controller = _fixture.CriarServicosController();
        var created = (CreatedAtActionResult)await controller.Post(ServicoSemItens(), CancellationToken.None);
        var id = ((ServicoViewModel)created.Value!).Id;

        // Act
        var resultado = await controller.Delete(id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(resultado);
    }

    [Fact]
    public async Task Delete_ServicoInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarServicosController();

        // Act
        var resultado = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }
}
