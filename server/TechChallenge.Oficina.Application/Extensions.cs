using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Oficina.UseCases.Features.Clientes.Commands;
using TechChallenge.Oficina.UseCases.Features.Clientes.Mappings;
using TechChallenge.Oficina.UseCases.Features.Clientes.Queries;
using TechChallenge.Oficina.UseCases.Features.Clientes.UseCases;
using TechChallenge.Oficina.UseCases.Features.Clientes.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Indicadores.Services;
using TechChallenge.Oficina.UseCases.Features.Insumos.Commands;
using TechChallenge.Oficina.UseCases.Features.Insumos.Queries;
using TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Insumos.ViewModels;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Commands;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Mappings;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.Queries;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Servicos.Commands;
using TechChallenge.Oficina.UseCases.Features.Servicos.Queries;
using TechChallenge.Oficina.UseCases.Features.Servicos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Servicos.ViewModels;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Commands;
using TechChallenge.Oficina.UseCases.Features.Veiculos.Queries;
using TechChallenge.Oficina.UseCases.Features.Veiculos.UseCases;
using TechChallenge.Oficina.UseCases.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Entities.Features.Clientes;
using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.Servicos;
using TechChallenge.Oficina.Entities.Features.Veiculos;
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
