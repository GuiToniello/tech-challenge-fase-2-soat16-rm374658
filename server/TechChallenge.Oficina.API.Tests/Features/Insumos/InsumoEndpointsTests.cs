using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using TechChallenge.Oficina.API.Features.Insumos;
using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.Queries;
using TechChallenge.Oficina.Application.Features.Insumos.Services;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.API.Tests.Features.Insumos;

public sealed class InsumoEndpointsTests
{
    private readonly Mock<IInsumoService> _serviceMock = new();

    [Fact]
    public async Task Post_DeveRetornarCreatedAtRoute_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var command = new CriarInsumoCommand { Nome = "Óleo", Fabricante = "Bosch", QuantidadeDisponivel = 10, ValorUnitario = 19.9m };
        var model = new InsumoViewModel { Id = Guid.NewGuid(), Nome = "Óleo" };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtRoute<InsumoViewModel>>(resultado.Result);
        Assert.Equal("GetInsumoById", created.RouteName);
        Assert.Equal(model, created.Value);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var endpoints = CriarEndpoints();
        var command = new CriarInsumoCommand { Nome = "", Fabricante = "Bosch", QuantidadeDisponivel = 10, ValorUnitario = 19.9m };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ThrowsAsync(new DomainException("erro"));

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("erro", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task GetById_DeveRetornarNotFound_QuandoNaoExiste()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.ObterPorIdAsync(It.Is<ObterInsumoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("não encontrado"));

        var resultado = await endpoints.GetById(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("não encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecao()
    {
        var endpoints = CriarEndpoints();
        IReadOnlyCollection<InsumoViewModel> insumos = [new InsumoViewModel { Nome = "Óleo" }];

        _serviceMock.Setup(s => s.ListarAsync(It.IsAny<ListarInsumosQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(insumos);

        var resultado = await endpoints.Get(CancellationToken.None);

        Assert.Equal(insumos, resultado.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        var endpoints = CriarEndpoints();
        var command = new AtualizarInsumoCommand { Id = Guid.NewGuid(), Nome = "Óleo", Fabricante = "Bosch", QuantidadeDisponivel = 10, ValorUnitario = 19.9m };

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

        var resultado = await endpoints.Delete(id, CancellationToken.None);

        Assert.IsType<NoContent>(resultado.Result);
        _serviceMock.Verify(s => s.ExcluirAsync(It.Is<ExcluirInsumoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    private InsumoEndpoints CriarEndpoints() => new(_serviceMock.Object);

    private static string? ObterMensagem(IReadOnlyDictionary<string, string?>? value)
    {
        return value is not null && value.TryGetValue("message", out var message) ? message : null;
    }
}
