using Microsoft.AspNetCore.Http.HttpResults;
using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Integration.Tests.Infrastructure;
using Xunit;

namespace TechChallenge.Oficina.Integration.Tests.Features.Insumos;

public sealed class InsumoEndpointsIntegrationTests : IDisposable
{
    private readonly IntegrationTestFixture _fixture;

    public InsumoEndpointsIntegrationTests()
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

    [Fact]
    public async Task Post_InsumoValido_DeveRetornar201ComViewModel()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        var command = InsumoValido();

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtRoute<InsumoViewModel>>(resultado.Result);
        var viewModel = Assert.IsType<InsumoViewModel>(created.Value);
        Assert.NotEqual(Guid.Empty, viewModel.Id);
        Assert.Equal("Óleo Lubrificante", viewModel.Nome);
        Assert.Equal(49.90m, viewModel.ValorUnitario);
    }

    [Fact]
    public async Task Post_NomeVazio_DeveRetornar400()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        var command = new CriarInsumoCommand { Nome = " ", Fabricante = "Mobil", QuantidadeDisponivel = 10, ValorUnitario = 10 };

        var resultado = await endpoints.Post(command, CancellationToken.None);

        Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado.Result);
    }

    [Fact]
    public async Task Post_ValorNegativo_DeveRetornar400()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        var command = new CriarInsumoCommand { Nome = "Filtro de Ar", Fabricante = "Mann", QuantidadeDisponivel = 10, ValorUnitario = -5m };

        var resultado = await endpoints.Post(command, CancellationToken.None);

        Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado.Result);
    }

    [Fact]
    public async Task GetById_InsumoExistente_DeveRetornar200ComViewModel()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        var created = (CreatedAtRoute<InsumoViewModel>)(await endpoints.Post(InsumoValido(), CancellationToken.None)).Result!;
        var id = created.Value.Id;

        var resultado = await endpoints.GetById(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<InsumoViewModel>>(resultado.Result);
        var viewModel = Assert.IsType<InsumoViewModel>(ok.Value);
        Assert.Equal(id, viewModel.Id);
    }

    [Fact]
    public async Task GetById_InsumoInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();

        var resultado = await endpoints.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
    }

    [Fact]
    public async Task Get_ComInsumos_DeveRetornar200ComLista()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        await endpoints.Post(InsumoValido("Vela de Ignição"), CancellationToken.None);

        var resultado = await endpoints.Get(CancellationToken.None);

        var lista = Assert.IsAssignableFrom<IEnumerable<InsumoViewModel>>(resultado.Value);
        Assert.NotEmpty(lista);
    }

    [Fact]
    public async Task Put_InsumoExistente_DeveRetornar200ComDadosAtualizados()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        var created = (CreatedAtRoute<InsumoViewModel>)(await endpoints.Post(InsumoValido(), CancellationToken.None)).Result!;
        var id = created.Value.Id;
        var command = new AtualizarInsumoCommand
        {
            Id = id,
            Nome = "Óleo Lubrificante Premium",
            Fabricante = "Castrol",
            QuantidadeDisponivel = 50,
            ValorUnitario = 79.90m
        };

        var resultado = await endpoints.Put(command, CancellationToken.None);

        var ok = Assert.IsType<Ok<InsumoViewModel>>(resultado.Result);
        var viewModel = Assert.IsType<InsumoViewModel>(ok.Value);
        Assert.Equal("Óleo Lubrificante Premium", viewModel.Nome);
        Assert.Equal(79.90m, viewModel.ValorUnitario);
    }

    [Fact]
    public async Task Put_InsumoInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        var command = new AtualizarInsumoCommand
        {
            Id = Guid.NewGuid(),
            Nome = "Qualquer",
            Fabricante = "Fab",
            QuantidadeDisponivel = 1,
            ValorUnitario = 1m
        };

        var resultado = await endpoints.Put(command, CancellationToken.None);

        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
    }

    [Fact]
    public async Task Delete_InsumoExistente_DeveRetornar204()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();
        var created = (CreatedAtRoute<InsumoViewModel>)(await endpoints.Post(InsumoValido(), CancellationToken.None)).Result!;
        var id = created.Value.Id;

        var resultado = await endpoints.Delete(id, CancellationToken.None);

        Assert.IsType<NoContent>(resultado.Result);
    }

    [Fact]
    public async Task Delete_InsumoInexistente_DeveRetornar404()
    {
        var endpoints = _fixture.CriarInsumosEndpoints();

        var resultado = await endpoints.Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
    }
}
