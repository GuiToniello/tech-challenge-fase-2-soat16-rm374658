using Moq;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Queries;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Exceptions;
using Xunit;

namespace TechChallenge.Oficina.Controllers.Tests.Features.OrdensServico;

public sealed class OrdensServicoControllerTests
{
    private readonly Mock<IOrdemServicoUseCases> _serviceMock = new();
    private readonly Mock<IOrdensServicoAdapter> _adapterMock = new();

    [Fact]
    public async Task Post_DeveRetornarAdaptado_QuandoSucesso()
    {
        var controller = CriarController();
        var command = new CriarOrdemServicoCommand { ClienteId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), ServicoIds = [Guid.NewGuid()] };
        var ordem = new OrdemServicoViewModel { Id = Guid.NewGuid() };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns(adaptedResult);

        var resultado = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, resultado);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>(), It.IsAny<bool>()), Times.Once);
    }

    [Fact]
    public async Task GetById_DeveRepassarIdCorretamente()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();
        var ordem = new OrdemServicoViewModel { Id = id };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.ObterPorIdAsync(It.Is<ObterOrdemServicoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordem);

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>()))
            .Returns(adaptedResult);

        var response = await controller.GetById(id, CancellationToken.None);

        Assert.Equal(adaptedResult, response);
        _serviceMock.Verify(service => service.ObterPorIdAsync(It.Is<ObterOrdemServicoPorIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_DeveRepassarQuery()
    {
        var controller = CriarController();
        IReadOnlyCollection<OrdemServicoViewModel> ordens = [new OrdemServicoViewModel { Id = Guid.NewGuid() }];
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.ListarAsync(It.IsAny<ListarOrdensServicoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ordens);

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<IReadOnlyCollection<OrdemServicoViewModel>, Exception>>()))
            .Returns(adaptedResult);

        var response = await controller.Get(CancellationToken.None);

        Assert.Equal(adaptedResult, response);
        _serviceMock.Verify(service => service.ListarAsync(It.IsAny<ListarOrdensServicoQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_DeveRepassarIdCorretamente()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.ExcluirAsync(It.Is<ExcluirOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _adapterMock
            .Setup(adapter => adapter.Empty())
            .Returns(adaptedResult);

        var response = await controller.Delete(id, CancellationToken.None);

        Assert.Equal(adaptedResult, response);
        _serviceMock.Verify(service => service.ExcluirAsync(It.Is<ExcluirOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GerarOrcamento_DeveRepassarComandoComId()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.GerarOrcamentoAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrdemServicoViewModel { Id = id });

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>()))
            .Returns(adaptedResult);

        var response = await controller.GerarOrcamento(id, CancellationToken.None);

        Assert.Equal(adaptedResult, response);
        _serviceMock.Verify(service => service.GerarOrcamentoAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarOrcamento_DeveRepassarIdCorretamente()
    {
        var controller = CriarController();
        var id = Guid.NewGuid();
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.EnviarOrcamentoPorEmailAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _adapterMock
            .Setup(adapter => adapter.Empty())
            .Returns(adaptedResult);

        var response = await controller.EnviarOrcamento(id, CancellationToken.None);

        Assert.Equal(adaptedResult, response);
        _serviceMock.Verify(service => service.EnviarOrcamentoPorEmailAsync(It.Is<AlterarStatusOrdemServicoCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Post_DeveRetornarAdaptado_QuandoDomainException()
    {
        var controller = CriarController();
        var command = new CriarOrdemServicoCommand { ClienteId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(), ServicoIds = [Guid.NewGuid()] };
        var adaptedResult = new object();

        _serviceMock
            .Setup(service => service.CriarAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("erro de domínio"));

        _adapterMock
            .Setup(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>(), It.IsAny<bool>()))
            .Returns(adaptedResult);

        var response = await controller.Post(command, CancellationToken.None);

        Assert.Equal(adaptedResult, response);
        _adapterMock.Verify(adapter => adapter.Adapt(It.IsAny<OrdensServicoResult<OrdemServicoViewModel, Exception>>(), It.IsAny<bool>()), Times.Once);
    }

    private OrdensServicoController CriarController()
    {
        return new OrdensServicoController(_serviceMock.Object, _adapterMock.Object);
    }
}
