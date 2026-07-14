using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using TechChallenge.Oficina.API.Features.OrdensServico;
using TechChallenge.Oficina.Application.Features.OrdensServico.Commands;
using TechChallenge.Oficina.Application.Features.OrdensServico.Queries;
using TechChallenge.Oficina.Application.Features.OrdensServico.Services;
using TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.API.Tests.Features.OrdensServico;

public sealed class OrdensServicoControllerTests
{
    private readonly Mock<IOrdemServicoService> _serviceMock = new();

    private OrdensServicoEndpoints CriarEndpoints() => new(_serviceMock.Object);

    [Fact]
    public async Task Post_DeveRetornarCreatedAtRoute_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var command = new CriarOrdemServicoCommand { ClienteId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), ServicoIds = [Guid.NewGuid()] };
        var model = new OrdemServicoViewModel { Id = Guid.NewGuid() };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var created = Assert.IsType<CreatedAtRoute<OrdemServicoViewModel>>(resultado.Result);
        Assert.Equal("GetOrdemServicoById", created.RouteName);
        Assert.Equal(model, created.Value);
    }

    [Fact]
    public async Task Post_DeveRetornarBadRequest_QuandoDomainException()
    {
        var endpoints = CriarEndpoints();
        var command = new CriarOrdemServicoCommand { ClienteId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), ServicoIds = [] };

        _serviceMock.Setup(s => s.CriarAsync(command, It.IsAny<CancellationToken>())).ThrowsAsync(new DomainException("erro"));

        var resultado = await endpoints.Post(command, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("erro", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task GetAcompanhamento_DeveRetornarOk_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();
        var model = new AcompanhamentoOrdemServicoViewModel { Id = id, Status = 1, StatusDescricao = "Recebida" };

        _serviceMock.Setup(s => s.ObterAcompanhamentoAsync(It.Is<ObterAcompanhamentoOrdemServicoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var resultado = await endpoints.GetAcompanhamento(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<AcompanhamentoOrdemServicoViewModel>>(resultado.Result);
        Assert.Equal(model, ok.Value);
    }

    [Fact]
    public async Task GetByCliente_DeveRetornarOkComColecao()
    {
        var endpoints = CriarEndpoints();
        var clienteId = Guid.NewGuid();
        IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel> model = [new AcompanhamentoOrdemServicoViewModel { Id = Guid.NewGuid(), Status = 1, StatusDescricao = "Recebida" }];

        _serviceMock.Setup(s => s.ListarPorClienteAsync(It.Is<ListarOrdensServicoPorClienteQuery>(q => q.ClienteId == clienteId), It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var resultado = await endpoints.GetByCliente(clienteId, CancellationToken.None);

        var ok = Assert.IsType<Ok<IReadOnlyCollection<AcompanhamentoOrdemServicoViewModel>>>(resultado.Result);
        Assert.Equal(model, ok.Value);
    }

    [Fact]
    public async Task Put_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        var endpoints = CriarEndpoints();
        var command = new AtualizarOrdemServicoCommand { Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), ServicoIds = [Guid.NewGuid()] };

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
        _serviceMock.Verify(s => s.ExcluirAsync(It.Is<ExcluirOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AlterarParaEmDiagnostico_DeveRetornarOk_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();
        var model = new OrdemServicoViewModel { Id = id, Status = 2, StatusDescricao = "Em diagnostico" };

        _serviceMock.Setup(s => s.AlterarStatusParaEmDiagnosticoAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var resultado = await endpoints.AlterarParaEmDiagnostico(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(resultado.Result);
        Assert.Equal(model, ok.Value);
    }

    [Fact]
    public async Task AlterarParaEmDiagnostico_DeveRetornarBadRequest_QuandoDomainException()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.AlterarStatusParaEmDiagnosticoAsync(It.IsAny<AlterarStatusOrdemServicoCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DomainException("transicao invalida"));

        var resultado = await endpoints.AlterarParaEmDiagnostico(id, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("transicao invalida", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task AlterarParaEmDiagnostico_DeveRetornarNotFound_QuandoNaoEncontrado()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.AlterarStatusParaEmDiagnosticoAsync(It.IsAny<AlterarStatusOrdemServicoCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("nao encontrado"));

        var resultado = await endpoints.AlterarParaEmDiagnostico(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("nao encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task AlterarParaEmExecucao_DeveRetornarOk_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();
        var model = new OrdemServicoViewModel { Id = id, Status = 4, StatusDescricao = "Em execucao" };

        _serviceMock.Setup(s => s.AlterarStatusParaEmExecucaoAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var resultado = await endpoints.AlterarParaEmExecucao(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(resultado.Result);
        Assert.Equal(model, ok.Value);
    }

    [Fact]
    public async Task AlterarParaFinalizada_DeveRetornarOk_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();
        var model = new OrdemServicoViewModel { Id = id, Status = 5, StatusDescricao = "Finalizada" };

        _serviceMock.Setup(s => s.AlterarStatusParaFinalizadaAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var resultado = await endpoints.AlterarParaFinalizada(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(resultado.Result);
        Assert.Equal(model, ok.Value);
    }

    [Fact]
    public async Task AlterarParaEntregue_DeveRetornarOk_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();
        var model = new OrdemServicoViewModel { Id = id, Status = 6, StatusDescricao = "Entregue" };

        _serviceMock.Setup(s => s.AlterarStatusParaEntregueAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var resultado = await endpoints.AlterarParaEntregue(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(resultado.Result);
        Assert.Equal(model, ok.Value);
    }

    [Fact]
    public async Task GerarOrcamento_DeveRetornarOk_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();
        var model = new OrdemServicoViewModel { Id = id, Status = 3, StatusDescricao = "Aguardando aprovacao" };

        _serviceMock.Setup(s => s.GerarOrcamentoAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var resultado = await endpoints.GerarOrcamento(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(resultado.Result);
        Assert.Equal(model, ok.Value);
    }

    [Fact]
    public async Task GerarOrcamento_DeveRetornarNotFound_QuandoOrdemNaoExiste()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.GerarOrcamentoAsync(It.IsAny<AlterarStatusOrdemServicoCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new KeyNotFoundException("nao encontrado"));

        var resultado = await endpoints.GerarOrcamento(id, CancellationToken.None);

        var notFound = Assert.IsType<NotFound<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("nao encontrado", ObterMensagem(notFound.Value));
    }

    [Fact]
    public async Task GerarOrcamento_DeveRetornarBadRequest_QuandoDomainException()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.GerarOrcamentoAsync(It.IsAny<AlterarStatusOrdemServicoCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DomainException("erro"));

        var resultado = await endpoints.GerarOrcamento(id, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("erro", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task EnviarOrcamento_DeveRetornarNoContent_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        var resultado = await endpoints.EnviarOrcamento(id, CancellationToken.None);

        Assert.IsType<NoContent>(resultado.Result);
        _serviceMock.Verify(s => s.EnviarOrcamentoPorEmailAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarOrcamento_DeveRetornarBadRequest_QuandoDomainException()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();

        _serviceMock.Setup(s => s.EnviarOrcamentoPorEmailAsync(It.IsAny<AlterarStatusOrdemServicoCommand>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DomainException("erro"));

        var resultado = await endpoints.EnviarOrcamento(id, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequest<Dictionary<string, string?>>>(resultado.Result);
        Assert.Equal("erro", ObterMensagem(badRequest.Value));
    }

    [Fact]
    public async Task AprovarOrcamento_DeveRetornarOk_QuandoSucesso()
    {
        var endpoints = CriarEndpoints();
        var id = Guid.NewGuid();
        var model = new OrdemServicoViewModel { Id = id, Status = 4, StatusDescricao = "Em execucao" };

        _serviceMock.Setup(s => s.AprovarOrcamentoAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>())).ReturnsAsync(model);

        var resultado = await endpoints.AprovarOrcamento(id, CancellationToken.None);

        var ok = Assert.IsType<Ok<OrdemServicoViewModel>>(resultado.Result);
        Assert.Equal(model, ok.Value);
    }

    private static string? ObterMensagem(IReadOnlyDictionary<string, string?>? value)
    {
        return value is not null && value.TryGetValue("message", out var message) ? message : null;
    }
}
