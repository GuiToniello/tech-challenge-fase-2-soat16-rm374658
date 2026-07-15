using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;
using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Application.Features.Indicadores.ViewModels;
using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Application.Features.OrdensServico.Commands;
using TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Application.Features.Servicos.Commands;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Application.Features.Veiculos.Commands;
using TechChallenge.Oficina.Application.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Integration.Tests.Infrastructure;

namespace TechChallenge.Oficina.Integration.Tests.Features.Indicadores;

public sealed class IndicadoresEndpointsIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;

    public IndicadoresEndpointsIntegrationTests()
    {
        _fixture = new IntegrationTestFixture();
    }

    public void Dispose() => _fixture.Dispose();

    // ------------------------------------------------------------------ //
    // GET - Obter indicadores
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Get_SemOrdensSevico_DeveRetornar200ComIndicadoresZerados()
    {
        // Arrange
        var endpoints = _fixture.CriarIndicadoresEndpoints();

        // Act
        var resultado = await endpoints.Get(CancellationToken.None);

        // Assert
        var viewModel = Assert.IsType<IndicadorViewModel>(resultado.Value);
        Assert.Equal(TimeSpan.Zero, viewModel.TempoMedioExecucao);
        Assert.Equal(TimeSpan.Zero, viewModel.TempoMedioEntrega);
    }

    [Fact]
    public async Task Get_ComOrdemEntregue_DeveRetornar200ComIndicadoresCalculados()
    {
        // Arrange
        var clienteId = await CriarClienteAsync();
        var veiculoId = await CriarVeiculoAsync(clienteId);
        var insumoId = await CriarInsumoAsync();
        var servicoId = await CriarServicoAsync(insumoId);

        var ordensServicoEndpoints = _fixture.CriarOrdensServicoEndpoints();
        var created = (CreatedAtRoute<OrdemServicoViewModel>)(await ordensServicoEndpoints.Post(
            new CriarOrdemServicoCommand { ClienteId = clienteId, VeiculoId = veiculoId, ServicoIds = [servicoId] },
            CancellationToken.None)).Result!;
        var ordemId = created.Value.Id;

        await ordensServicoEndpoints.AlterarParaEmDiagnostico(ordemId, CancellationToken.None);
        await ordensServicoEndpoints.GerarOrcamento(ordemId, CancellationToken.None);
        await ordensServicoEndpoints.AprovarOrcamento(ordemId, CancellationToken.None);
        await ordensServicoEndpoints.AlterarParaEmExecucao(ordemId, CancellationToken.None);
        await ordensServicoEndpoints.AlterarParaFinalizada(ordemId, CancellationToken.None);
        await ordensServicoEndpoints.AlterarParaEntregue(ordemId, CancellationToken.None);

        var endpoints = _fixture.CriarIndicadoresEndpoints();

        // Act
        var resultado = await endpoints.Get(CancellationToken.None);

        // Assert
        Assert.IsType<IndicadorViewModel>(resultado.Value);
    }

    // ------------------------------------------------------------------ //
    // Helpers
    // ------------------------------------------------------------------ //

    private async Task<Guid> CriarClienteAsync()
    {
        var clientEndpoints = _fixture.CriarClientesEndpoints();
        var result = Assert.IsType<CreatedAtRoute<ClienteViewModel>>(await clientEndpoints.Post(
            new CriarClienteCommand { NomeCompleto = "Cliente Indicador", Identificacao = "529.982.247-25" },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private async Task<Guid> CriarVeiculoAsync(Guid clienteId)
    {
        var endpoints = _fixture.CriarVeiculosEndpoints();
        var result = Assert.IsType<CreatedAtRoute<VeiculoViewModel>>(await endpoints.Post(
            new CriarVeiculoCommand { Placa = "DEF2G45", Marca = "VW", Modelo = "Gol", Ano = 2021, Renavam = "11122233344", ClienteId = clienteId },
            CancellationToken.None));
        return result.Value!.Id;
    }

    private async Task<Guid> CriarInsumoAsync()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        var result = (CreatedAtRoute<InsumoViewModel>)(await endpoints.Post(
            new CriarInsumoCommand { Nome = "Correia Dentada", Fabricante = "Gates", QuantidadeDisponivel = 30, ValorUnitario = 85m },
            CancellationToken.None)).Result!;
        return result.Value.Id;
    }

    private async Task<Guid> CriarServicoAsync(Guid insumoId)
    {
        var endpoints = _fixture.CriarServicosEndpoints();
        var result = (CreatedAtRoute<ServicoViewModel>)(await endpoints.Post(
            new CriarServicoCommand
            {
                Nome = "Troca de Correia",
                Descricao = "Substituição da correia dentada",
                ItensServico = [new ItemServicoCommand { InsumoId = insumoId, Quantidade = 1 }]
            },
            CancellationToken.None)).Result!;
        return result.Value.Id;
    }
}
