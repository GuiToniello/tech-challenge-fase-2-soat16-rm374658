using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using TechChallenge.Oficina.API.Features.Servicos;
using TechChallenge.Oficina.Application.Features.Servicos.Commands;
using TechChallenge.Oficina.Application.Features.Servicos.Queries;
using TechChallenge.Oficina.Application.Features.Servicos.Services;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.API.Tests.Features.Servicos;

public sealed class ServicosControllerTests
{
    private readonly Mock<IServicoService> _serviceMock = new();

    private ServicoEndpoints CriarEndpoints() => new(_serviceMock.Object);

    [Fact]
    public async Task Post_DeveRetornarCreatedAtRoute_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var command = new CriarServicoCommand { Nome = "Troca", Descricao = "Descricao", ItensServico = [] };
        var model = new ServicoViewModel { Id = Guid.NewGuid(), Nome = "Troca" };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtRoute<ServicoViewModel>>(resultado.Result);
        Assert.Equal("GetServicoById", created.RouteName);
        Assert.Equal(model, created.Value);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var endpoints = CriarEndpoints();
        var command = new CriarServicoCommand { Nome = "", Descricao = "Descricao", ItensServico = [] };

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

        _serviceMock.Setup(s => s.ObterPorIdAsync(It.Is<ObterServicoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("nao encontrado"));

        var resultado = await endpoints.GetById(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("nao encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Get_DeveRetornarOkComColecao()
    {
        var endpoints = CriarEndpoints();
        IReadOnlyCollection<ServicoViewModel> servicos = [new ServicoViewModel { Nome = "Troca" }];

        _serviceMock.Setup(s => s.ListarAsync(It.IsAny<ListarServicosQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(servicos);

        var resultado = await endpoints.Get(CancellationToken.None);

        var ok = Assert.IsType<Ok<IReadOnlyCollection<ServicoViewModel>>>(resultado);
        Assert.Equal(servicos, ok.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        var endpoints = CriarEndpoints();
        var command = new AtualizarServicoCommand { Id = Guid.NewGuid(), Nome = "Troca", Descricao = "Descricao", ItensServico = [] };

        _serviceMock.Setup(s => s.AtualizarAsync(command, It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("nao encontrado"));

        var resultado = await endpoints.Put(command, CancellationToken.None);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("nao encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task Delete_DeveRetornarNoContent_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        var resultado = await endpoints.Delete(id, CancellationToken.None);

        Assert.IsType<NoContent>(resultado.Result);
        _serviceMock.Verify(s => s.ExcluirAsync(It.Is<ExcluirServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static string? ObterMensagem(IReadOnlyDictionary<string, string?>? value)
    {
        return value is not null && value.TryGetValue("message", out var message) ? message : null;
    }
}
