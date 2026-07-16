using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Oficina.UseCases.Features.Clientes.Mappings;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Indicadores.Services;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Mappings;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;

namespace TechChallenge.Oficina.UseCases;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(ClienteProfile).Assembly, typeof(OrdemServicoProfile).Assembly);
        services.AddScoped<IClienteUseCases, ClienteUseCases>();
        services.AddScoped<IIndicadorUseCases, IndicadorUseCases>();
        services.AddScoped<IInsumoUseCases, InsumoUseCases>();
        services.AddScoped<IEstoqueUseCases, EstoqueUseCases>();
        services.AddScoped<IOrdemServicoUseCasesFacade, OrdemServicoUseCasesFacade>();
        services.AddScoped<IOrdemServicoUseCases, OrdemServicoUseCases>();
        services.AddScoped<IServicoUseCases, ServicoUseCases>();
        services.AddScoped<IVeiculoUseCases, VeiculoUseCases>();
        return services;
    }
}
