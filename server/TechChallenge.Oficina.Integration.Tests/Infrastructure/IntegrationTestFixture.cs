using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TechChallenge.Oficina.UseCases;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;
using TechChallenge.Oficina.API.Features.Clientes;
using TechChallenge.Oficina.API.Features.Indicadores;
using TechChallenge.Oficina.API.Features.Insumos;
using TechChallenge.Oficina.API.Features.OrdensServico;
using TechChallenge.Oficina.API.Features.Servicos;
using TechChallenge.Oficina.API.Features.Veiculos;
using TechChallenge.Oficina.Controllers.Features.Clientes;
using TechChallenge.Oficina.Controllers.Features.Indicadores;
using TechChallenge.Oficina.Controllers.Features.Insumos;
using TechChallenge.Oficina.Controllers.Features.OrdensServico;
using TechChallenge.Oficina.Controllers.Features.Servicos;
using TechChallenge.Oficina.Controllers.Features.Veiculos;
using TechChallenge.Oficina.Entities.Features.Clientes;
using TechChallenge.Oficina.Entities.Features.Indicadores;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.Orcamentos;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.Servicos;
using TechChallenge.Oficina.Entities.Features.Veiculos;
using TechChallenge.Oficina.DB.Data.Context;
using TechChallenge.Oficina.DB.Data.Features.Clientes;
using TechChallenge.Oficina.DB.Data.Features.Indicadores;
using TechChallenge.Oficina.DB.Data.Features.Insumos;
using TechChallenge.Oficina.DB.Data.Features.OrdensServico;
using TechChallenge.Oficina.DB.Data.Features.Servicos;
using TechChallenge.Oficina.DB.Data.Features.Veiculos;

namespace TechChallenge.Oficina.Integration.Tests.Infrastructure;

/// <summary>
/// Fixture de integração: DI real com EF Core InMemory e mock apenas de IOrcamentoEmailSender.
/// Cada instância possui banco de dados isolado (nome único por GUID).
/// </summary>
public sealed class IntegrationTestFixture : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;

    public IntegrationTestFixture()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        var dbName = Guid.NewGuid().ToString();
        services.AddDbContext<OficinaDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IInsumoRepository, InsumoRepository>();
        services.AddScoped<IIndicadorRepository, IndicadorRepository>();
        services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
        services.AddScoped<IServicoRepository, ServicoRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();

        var emailSenderMock = new Mock<IOrcamentoEmailSender>();
        emailSenderMock
            .Setup(s => s.EnviarOrcamentoAsync(
                It.IsAny<OrdemServico>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        services.AddSingleton(emailSenderMock.Object);

        services.AddApplication();
        services.AddScoped<IClienteAdapter, ClienteAdapter>();
        services.AddScoped<ClienteController>();
        services.AddScoped<IIndicadoresAdapter, IndicadoresAdapter>();
        services.AddScoped<IIndicadoresController, IndicadoresController>();
        services.AddScoped<IServicoAdapter, ServicoAdapter>();
        services.AddScoped<ServicoController>();
        services.AddScoped<IOrdensServicoAdapter, OrdensServicoAdapter>();
        services.AddScoped<OrdensServicoController>();
        services.AddScoped<IVeiculoAdapter, VeiculoAdapter>();
        services.AddScoped<VeiculoController>();
        services.AddScoped<InsumoController>();
        services.AddScoped<IInsumoAdapter, InsumoAdapter>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
    }

    private T Obter<T>() where T : notnull
        => _scope.ServiceProvider.GetRequiredService<T>();

    public ClienteController CriarClientesEndpoints()
        => Obter<ClienteController>();

    public InsumoController CriarInsumosEndpoints()
        => Obter<InsumoController>();

    public VeiculoController CriarVeiculosEndpoints()
        => Obter<VeiculoController>();

    public ServicoController CriarServicosEndpoints()
        => Obter<ServicoController>();

    public OrdensServicoController CriarOrdensServicoEndpoints()
        => Obter<OrdensServicoController>();

    public IIndicadoresController CriarIndicadoresEndpoints()
        => Obter<IIndicadoresController>();

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
    }
}

