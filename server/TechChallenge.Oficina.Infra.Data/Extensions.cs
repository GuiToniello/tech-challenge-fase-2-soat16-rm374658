using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Oficina.Domain.Features.Clientes;
using TechChallenge.Oficina.Domain.Features.Insumos;
using TechChallenge.Oficina.Domain.Features.Indicadores;
using TechChallenge.Oficina.Domain.Features.OrdensServico;
using TechChallenge.Oficina.Domain.Features.Servicos;
using TechChallenge.Oficina.Domain.Features.Veiculos;
using TechChallenge.Oficina.Infra.Data.Context;
using TechChallenge.Oficina.Infra.Data.Features.Clientes;
using TechChallenge.Oficina.Infra.Data.Features.Insumos;
using TechChallenge.Oficina.Infra.Data.Features.Indicadores;
using TechChallenge.Oficina.Infra.Data.Features.OrdensServico;
using TechChallenge.Oficina.Infra.Data.Features.Servicos;
using TechChallenge.Oficina.Infra.Data.Features.Veiculos;

namespace TechChallenge.Oficina.Infra.Data;

public static class Extensions
{
    public static IServiceCollection AddInfraData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<OficinaDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IInsumoRepository, InsumoRepository>();
        services.AddScoped<IIndicadorRepository, IndicadorRepository>();
        services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
        services.AddScoped<IServicoRepository, ServicoRepository>();
        services.AddScoped<IVeiculoRepository, VeiculoRepository>();
        return services;
    }

    public static void ApplyMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OficinaDbContext>();
        dbContext.Database.Migrate();
    }
}
