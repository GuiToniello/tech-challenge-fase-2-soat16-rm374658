using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TechChallenge.Oficina.Application;
using TechChallenge.Oficina.Application.Features.Clientes.Services;
using TechChallenge.Oficina.Application.Features.Insumos.Services;
using TechChallenge.Oficina.Application.Features.OrdensServico.Services;
using TechChallenge.Oficina.Application.Features.Servicos.Services;
using TechChallenge.Oficina.Application.Features.Veiculos.Services;
using TechChallenge.Oficina.API.Features.Clientes;
using TechChallenge.Oficina.API.Features.Indicadores;
using TechChallenge.Oficina.API.Features.Insumos;
using TechChallenge.Oficina.API.Features.OrdensServico;
using TechChallenge.Oficina.API.Features.Servicos;
using TechChallenge.Oficina.API.Features.Veiculos;
using TechChallenge.Oficina.Domain.Features.Clientes;
using TechChallenge.Oficina.Domain.Features.Indicadores;
using TechChallenge.Oficina.Domain.Features.Insumos;
using TechChallenge.Oficina.Domain.Features.Orcamentos;
using TechChallenge.Oficina.Domain.Features.OrdensServico;
using TechChallenge.Oficina.Domain.Features.Servicos;
using TechChallenge.Oficina.Domain.Features.Veiculos;
using TechChallenge.Oficina.Infra.Data.Context;
using TechChallenge.Oficina.Infra.Data.Features.Clientes;
using TechChallenge.Oficina.Infra.Data.Features.Indicadores;
using TechChallenge.Oficina.Infra.Data.Features.Insumos;
using TechChallenge.Oficina.Infra.Data.Features.OrdensServico;
using TechChallenge.Oficina.Infra.Data.Features.Servicos;
using TechChallenge.Oficina.Infra.Data.Features.Veiculos;
using TechChallenge.Oficina.Controllers.Features.Clientes;
using TechChallenge.Oficina.Controllers.Features.Servicos;
using TechChallenge.Oficina.Controllers.Features.Veiculos;

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
        services.AddScoped<IClientAdapter, ClienteAdapter>();
        services.AddScoped<ClienteController>();
        services.AddScoped<IServicoAdapter, ServicoAdapter>();
        services.AddScoped<ServicoController>();
        services.AddScoped<IVeiculoAdapter, VeiculoAdapter>();
        services.AddScoped<VeiculoController>();
        services.AddScoped<IndicadoresEndpoints>();
        services.AddScoped<InsumoEndpoints>();
        services.AddScoped<OrdensServicoEndpoints>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
    }

    private T Obter<T>() where T : notnull
        => _scope.ServiceProvider.GetRequiredService<T>();

    public ClienteController CriarClientesEndpoints()
        => Obter<ClienteController>();

    public InsumoEndpoints CriarInsumosEndpoints()
        => Obter<InsumoEndpoints>();

    public VeiculoController CriarVeiculosEndpoints()
        => Obter<VeiculoController>();

    public ServicoController CriarServicosEndpoints()
        => Obter<ServicoController>();

    public OrdensServicoEndpoints CriarOrdensServicoEndpoints()
        => Obter<OrdensServicoEndpoints>();

    public IndicadoresEndpoints CriarIndicadoresEndpoints()
        => Obter<IndicadoresEndpoints>();

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
    }
}

