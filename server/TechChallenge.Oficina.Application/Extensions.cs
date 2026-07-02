using Microsoft.Extensions.DependencyInjection;
using TechChallenge.Oficina.Application.Features.Clientes.Commands;
using TechChallenge.Oficina.Application.Features.Clientes.Mappings;
using TechChallenge.Oficina.Application.Features.Clientes.Queries;
using TechChallenge.Oficina.Application.Features.Clientes.Services;
using TechChallenge.Oficina.Application.Features.Clientes.ViewModels;
using TechChallenge.Oficina.Application.Features.Indicadores;
using TechChallenge.Oficina.Application.Features.Indicadores.Services;
using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.Queries;
using TechChallenge.Oficina.Application.Features.Insumos.Services;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Application.Features.OrdensServico.Commands;
using TechChallenge.Oficina.Application.Features.OrdensServico.Mappings;
using TechChallenge.Oficina.Application.Features.OrdensServico.Queries;
using TechChallenge.Oficina.Application.Features.OrdensServico.Services;
using TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;
using TechChallenge.Oficina.Application.Features.Servicos.Commands;
using TechChallenge.Oficina.Application.Features.Servicos.Queries;
using TechChallenge.Oficina.Application.Features.Servicos.Services;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Application.Features.Veiculos.Commands;
using TechChallenge.Oficina.Application.Features.Veiculos.Queries;
using TechChallenge.Oficina.Application.Features.Veiculos.Services;
using TechChallenge.Oficina.Application.Features.Veiculos.ViewModels;
using TechChallenge.Oficina.Domain.Features.Clientes;
using TechChallenge.Oficina.Domain.Features.Insumos;
using TechChallenge.Oficina.Domain.Features.OrdensServico;
using TechChallenge.Oficina.Domain.Features.Servicos;
using TechChallenge.Oficina.Domain.Features.Veiculos;

namespace TechChallenge.Oficina.Application;

public static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => { }, typeof(ClienteProfile).Assembly, typeof(OrdemServicoProfile).Assembly);
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IIndicadorService, IndicadorService>();
        services.AddScoped<IInsumoService, InsumoService>();
        services.AddScoped<IEstoqueService, EstoqueService>();
        services.AddScoped<IOrdemServicoServicesFacade, OrdemServicoServicesFacade>();
        services.AddScoped<IOrdemServicoService, OrdemServicoService>();
        services.AddScoped<IServicoService, ServicoService>();
        services.AddScoped<IVeiculoService, VeiculoService>();
        return services;
    }
}
