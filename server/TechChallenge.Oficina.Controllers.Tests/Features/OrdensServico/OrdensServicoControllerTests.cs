using Moq;
using TechChallenge.Oficina.Application.Features.OrdensServico.Commands;
using TechChallenge.Oficina.Application.Features.OrdensServico.Queries;
using TechChallenge.Oficina.Application.Features.OrdensServico.Services;
using TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.Domain.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Controllers.Tests.Features.OrdensServico;

public sealed class OrdensServicoControllerTests
{
    private readonly Mock<IOrdemServicoService> _serviceMock = new();
    private readonly Mock<IOrdensServicoAdapter> _adapterMock = new();

    [Fact]
    public async Task Post_DeveRetornarAdaptado_QuandoSucesso()
    {
        ConfigurarAdapterParaOrdemResult();
        var controller = CriarController();
        var command = new CriarOrdemServicoCommand { ClienteId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), ServicoIds = [Guid.NewGuid()] };
        var ordem = new OrdemServicoViewModel { Id = Guid.NewGuid() };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>(), true))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>(), true), Times.Once);
    }

    [Fact]
    public async Task GetById_DeveRepassarIdCorretamente()
    {
        ConfigurarAdapterParaOrdemResult();
        var controller = CriarController();
        var id = Guid.NewGuid();
        var ordem = new OrdemServicoViewModel { Id = id };

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterOrdemServicoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        var response = await controller.GetById(id, CancellationToken.None);
        var resultado = Assert.IsType<OrdensServicoResult<OrdemServicoViewModel, Exception>>(response);

        Assert.NotNull(resultado.Value);
        Assert.Null(resultado.Error);
        Assert.Equal(ordem, resultado.Value);
        _serviceMock.Verify(service => service.ObterPorIdAsync(It.Is<ObterOrdemServicoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_DeveRepassarQuery()
    {
        ConfigurarAdapterParaColecaoResult();
        var controller = CriarController();
        IReadOnlyCollection<OrdemServicoViewModel> ordens = [new OrdemServicoViewModel { Id = Guid.NewGuid() }];

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarOrdensServicoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordens);

        await controller.Get(CancellationToken.None);

        _serviceMock.Verify(service => service.ListarAsync(It.IsAny<ListarOrdensServicoQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_DeveRepassarIdCorretamente()
    {
        ConfigurarAdapterParaBoolResult();
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.ExcluirAsync(It.Is<ExcluirOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await controller.Delete(id, CancellationToken.None);

        _serviceMock.Verify(service => service.ExcluirAsync(It.Is<ExcluirOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GerarOrcamento_DeveRepassarComandoComId()
    {
        ConfigurarAdapterParaOrdemResult();
        var controller = CriarController();
        var id = Guid.NewGuid();
        var ordem = new OrdemServicoViewModel { Id = id };

        _serviceMock
            .Setup(service => service.GerarOrcamentoAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        await controller.GerarOrcamento(id, CancellationToken.None);

        _serviceMock.Verify(service => service.GerarOrcamentoAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarOrcamento_DeveRepassarIdCorretamente()
    {
        ConfigurarAdapterParaEmpty();
        var controller = CriarController();
        var id = Guid.NewGuid();

        _serviceMock
            .Setup(service => service.EnviarOrcamentoPorEmailAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await controller.EnviarOrcamento(id, CancellationToken.None);

        _serviceMock.Verify(service => service.EnviarOrcamentoPorEmailAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_DeveRetornarAdaptado_QuandoDomainException()
    {
        ConfigurarAdapterParaOrdemResult();
        var controller = CriarController();
        var command = new CriarOrdemServicoCommand { ClienteId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), ServicoIds = [Guid.NewGuid()] };

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns(new object());

        await controller.Post(command, CancellationToken.None);

        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>(), It.IsAny<bool>()), Times.Once);
    }

    private void ConfigurarAdapterParaOrdemResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns((OrdensServicoResult<OrdemServicoViewModel, Exception> result, bool _) => result);
    }

    private void ConfigurarAdapterParaColecaoResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<IReadOnlyCollection<OrdemServicoViewModel>, Exception>>()))
            .Returns((OrdensServicoResult<IReadOnlyCollection<OrdemServicoViewModel>, Exception> result) => result);
    }

    private void ConfigurarAdapterParaBoolResult()
    {
        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<bool, Exception>>()))
            .Returns((OrdensServicoResult<bool, Exception> result) => result);
    }

    private void ConfigurarAdapterParaEmpty()
    {
        _adapterMock
            .Setup(adapter => adapter.Empty())
            .Returns(new object());
    }

    private OrdensServicoController CriarController()
    {
        return new OrdensServicoController(_serviceMock.Object, _adapterMock.Object);
    }
}
