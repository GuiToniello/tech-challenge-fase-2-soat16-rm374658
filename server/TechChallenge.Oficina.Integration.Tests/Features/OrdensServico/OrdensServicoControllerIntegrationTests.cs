using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Application.Features.OrdensServico.Commands;
using TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Application.Features.Servicos.Commands;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Application.Features.Veiculos.Commands;
using TechChallenge.Oficina.Application.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Integration.Tests.Infrastructure;

namespace TechChallenge.Oficina.Integration.Tests.Features.OrdensServico;

public sealed class OrdensServicoControllerIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;

    public OrdensServicoControllerIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public void Dispose() => _fixture.Dispose();

    private async Task<Guid> CriarClienteComEmailAsync()
    {
        var endpoints = _fixture.CriarClientesEndpoints();
        // Usa CPF único baseado em tempo para evitar duplicidade no banco compartilhado
        var cpf = _clienteCpfIndex++ == 0 ? "529.982.247-25" : "123.456.789-09";
        var result = (CreatedAtRoute<ClienteViewModel>)(await endpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente OS", Identificacao = cpf, Email = $"os{cpf[..3]}@email.com" },
            CancellationToken.None)).Result!;
        return result.Value.Id;
    }

    private int _clienteCpfIndex;

    private async Task<Guid> CriarVeiculoAsync(Guid clienteId)
    {
        var controller = _fixture.CriarVeiculosController();
        var result = (CreatedAtActionResult)await controller.Post(
            new CriarVeiculoCommand { Placa = "ABC1D23", Marca = "Ford", Modelo = "Ka", Ano = 2022, Renavam = "12345678901", ClienteId = clienteId },
            CancellationToken.None);
        return ((VeiculoViewModel)result.Value!).Id;
    }

    private async Task<Guid> CriarInsumoAsync()
    {
        var controller = _fixture.CriarInsumosController();
        var result = (CreatedAtActionResult)await controller.Post(
            new CriarInsumoCommand { Nome = "Vela de Ignição", Fabricante = "NGK", QuantidadeDisponivel = 20, ValorUnitario = 15m },
            CancellationToken.None);
        return ((InsumoViewModel)result.Value!).Id;
    }

    private async Task<Guid> CriarServicoAsync(Guid insumoId)
    {
        var controller = _fixture.CriarServicosController();
        var result = (CreatedAtActionResult)await controller.Post(
            new CriarServicoCommand
            {
                Nome = "Troca de Vela",
                Descricao = "Substituição das velas de ignição",
                ItensServico = [new ItemServicoCommand { InsumoId = insumoId, Quantidade = 4 }]
            },
            CancellationToken.None);
        return ((ServicoViewModel)result.Value!).Id;
    }

    private async Task<(Guid clienteId, Guid veiculoId, Guid servicoId)> CriarContextoCompletoAsync()
    {
        var clienteId = await CriarClienteComEmailAsync();
        var veiculoId = await CriarVeiculoAsync(clienteId);
        var insumoId = await CriarInsumoAsync();
        var servicoId = await CriarServicoAsync(insumoId);
        return (clienteId, veiculoId, servicoId);
    }

    // ------------------------------------------------------------------ //
    // POST - Criar
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Post_OrdemServicoValida_DeveRetornar201ComViewModel()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var controller = _fixture.CriarOrdensServicoController();
        var command = new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] };

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(resultado);
        var viewModel = Assert.IsType<OrdemServicoViewModel>(created.Value);
        Assert.NotEqual(Guid.Empty, viewModel.Id);
        Assert.Equal(clienteId, viewModel.ClienteId);
        Assert.Equal(veiculoId, viewModel.VeiculoId);
    }

    [Fact]
    public async Task Post_ClienteInexistente_DeveRetornar404()
    {
        // Arrange
        var (_, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var controller = _fixture.CriarOrdensServicoController();
        var command = new CriarOrdemServicoCommand { ClienteId = Guid.NewGuid(), VeiculoId = veiculoId, ServicoIds = [servicoId] };

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    [Fact]
    public async Task Post_SemServicos_DeveRetornar400()
    {
        // Arrange
        var (clienteId, veiculoId, _) = await CriarContextoCompletoAsync();
        var controller = _fixture.CriarOrdensServicoController();
        var command = new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [] };

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    [Fact]
    public async Task Post_VeiculoNaoPertenceAoCliente_DeveRetornar400()
    {
        // Arrange - cliente 1 (proprietário real do veículo)
        var clientEndpoints = _fixture.CriarClientesEndpoints();
        var cliente1Result = (CreatedAtRoute<ClienteViewModel>)(await clientEndpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente Dono", Identificacao = "529.982.247-25", Email = "dono@email.com" },
            CancellationToken.None)).Result!;
        var clienteDono = cliente1Result.Value.Id;

        // cliente 2 (criará a ordem mas não é dono do veículo)
        var cliente2Result = (CreatedAtRoute<ClienteViewModel>)(await clientEndpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente Ordem", Identificacao = "123.456.789-09", Email = "ordem@email.com" },
            CancellationToken.None)).Result!;
        var clienteOrdem = cliente2Result.Value.Id;

        // veículo pertence ao clienteDono
        var veiculoController = _fixture.CriarVeiculosController();
        var veiculoResult = (CreatedAtActionResult)await veiculoController.Post(
            new CriarVeiculoCommand { Placa = "QQQ1Q11", Marca = "Fiat", Modelo = "Uno", Ano = 2020, Renavam = "00011122233", ClienteId = clienteDono },
            CancellationToken.None);
        var veiculoDoCliente1 = ((VeiculoViewModel)veiculoResult.Value!).Id;

        // insumo e serviço para completar o comando
        var insumoController = _fixture.CriarInsumosController();
        var insumoResult = (CreatedAtActionResult)await insumoController.Post(
            new CriarInsumoCommand { Nome = "Pastilha de Freio", Fabricante = "Bosch", QuantidadeDisponivel = 10, ValorUnitario = 40m },
            CancellationToken.None);
        var insumoId = ((InsumoViewModel)insumoResult.Value!).Id;

        var servicoController = _fixture.CriarServicosController();
        var servicoResult = (CreatedAtActionResult)await servicoController.Post(
            new CriarServicoCommand { Nome = "Troca Freio", Descricao = "Troca pastilha de freio", ItensServico = [new ItemServicoCommand { InsumoId = insumoId, Quantidade = 2 }] },
            CancellationToken.None);
        var servicoId = ((ServicoViewModel)servicoResult.Value!).Id;

        // ordem criada com clienteOrdem mas veículo do clienteDono
        var controller = _fixture.CriarOrdensServicoController();
        var command = new CriarOrdemServicoCommand { ClienteId = clienteOrdem, VeiculoId = veiculoDoCliente1, ServicoIds = [servicoId] };

        // Act
        var resultado = await controller.Post(command, CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(resultado);
    }

    // ------------------------------------------------------------------ //
    // GET por id
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetById_OrdemExistente_DeveRetornar200ComViewModel()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var controller = _fixture.CriarOrdensServicoController();
        var created = (CreatedAtActionResult)await controller.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None);
        var id = ((OrdemServicoViewModel)created.Value!).Id;

        // Act
        var resultado = await controller.GetById(id, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.IsType<OrdemServicoViewModel>(ok.Value);
    }

    [Fact]
    public async Task GetById_OrdemInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarOrdensServicoController();

        // Act
        var resultado = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    // ------------------------------------------------------------------ //
    // GET lista
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Get_ComOrdensServico_DeveRetornar200ComLista()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var controller = _fixture.CriarOrdensServicoController();
        await controller.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None);

        // Act
        var resultado = await controller.Get(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var lista = Assert.IsAssignableFrom<IEnumerable<OrdemServicoViewModel>>(ok.Value);
        Assert.NotEmpty(lista);
    }

    // ------------------------------------------------------------------ //
    // GET acompanhamento
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetAcompanhamento_OrdemExistente_DeveRetornar200()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var controller = _fixture.CriarOrdensServicoController();
        var created = (CreatedAtActionResult)await controller.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None);
        var id = ((OrdemServicoViewModel)created.Value!).Id;

        // Act
        var resultado = await controller.GetAcompanhamento(id, CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(resultado);
    }

    // ------------------------------------------------------------------ //
    // GET por cliente
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetByCliente_ClienteExistente_DeveRetornar200ComLista()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var controller = _fixture.CriarOrdensServicoController();
        await controller.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None);

        // Act
        var resultado = await controller.GetByCliente(clienteId, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var lista = Assert.IsAssignableFrom<IEnumerable<AcompanhamentoOrdemServicoViewModel>>(ok.Value);
        Assert.NotEmpty(lista);
    }

    // ------------------------------------------------------------------ //
    // Fluxo de status
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task AlterarParaEmDiagnostico_OrdemRecebida_DeveRetornar200ComStatusAtualizado()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var controller = _fixture.CriarOrdensServicoController();
        var created = (CreatedAtActionResult)await controller.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None);
        var id = ((OrdemServicoViewModel)created.Value!).Id;

        // Act
        var resultado = await controller.AlterarParaEmDiagnostico(id, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var viewModel = Assert.IsType<OrdemServicoViewModel>(ok.Value);
        Assert.Equal(2, viewModel.Status); // EmDiagnostico
    }

    [Fact]
    public async Task GerarOrcamento_OrdemEmDiagnostico_DeveRetornar200()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var controller = _fixture.CriarOrdensServicoController();
        var created = (CreatedAtActionResult)await controller.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None);
        var id = ((OrdemServicoViewModel)created.Value!).Id;
        await controller.AlterarParaEmDiagnostico(id, CancellationToken.None);

        // Act
        var resultado = await controller.GerarOrcamento(id, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        var viewModel = Assert.IsType<OrdemServicoViewModel>(ok.Value);
        Assert.NotNull(viewModel.Orcamento);
    }

    [Fact]
    public async Task FluxoCompleto_DevePassarPorTodosOsStatus()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var controller = _fixture.CriarOrdensServicoController();
        var created = (CreatedAtActionResult)await controller.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None);
        var id = ((OrdemServicoViewModel)created.Value!).Id;

        // Act & Assert - fluxo completo
        await controller.AlterarParaEmDiagnostico(id, CancellationToken.None);
        await controller.GerarOrcamento(id, CancellationToken.None);
        await controller.AprovarOrcamento(id, CancellationToken.None);
        await controller.AlterarParaEmExecucao(id, CancellationToken.None);
        await controller.AlterarParaFinalizada(id, CancellationToken.None);
        var entregue = await controller.AlterarParaEntregue(id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(entregue);
        var viewModel = Assert.IsType<OrdemServicoViewModel>(ok.Value);
        Assert.Equal(6, viewModel.Status); // Entregue
    }

    // ------------------------------------------------------------------ //
    // DELETE
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Delete_OrdemExistente_DeveRetornar204()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var controller = _fixture.CriarOrdensServicoController();
        var created = (CreatedAtActionResult)await controller.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None);
        var id = ((OrdemServicoViewModel)created.Value!).Id;

        // Act
        var resultado = await controller.Delete(id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(resultado);
    }

    [Fact]
    public async Task Delete_OrdemInexistente_DeveRetornar404()
    {
        // Arrange
        var controller = _fixture.CriarOrdensServicoController();

        // Act
        var resultado = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }

    // ------------------------------------------------------------------ //
    // PUT - Atualizar
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Put_OrdemExistente_DeveRetornar200()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var controller = _fixture.CriarOrdensServicoController();
        var created = (CreatedAtActionResult)await controller.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None);
        var id = ((OrdemServicoViewModel)created.Value!).Id;
        var command = new AtualizarOrdemServicoCommand { Id = id, ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] };

        // Act
        var resultado = await controller.Put(command, CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resultado);
        Assert.IsType<OrdemServicoViewModel>(ok.Value);
    }

    [Fact]
    public async Task Put_OrdemInexistente_DeveRetornar404()
    {
        // Arrange
        var (clienteId, veiculoId, servicoId) = await CriarContextoCompletoAsync();
        var controller = _fixture.CriarOrdensServicoController();
        var command = new AtualizarOrdemServicoCommand { Id = Guid.NewGuid(), ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] };

        // Act
        var resultado = await controller.Put(command, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resultado);
    }
}
