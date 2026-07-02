using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Domain.Features.Clientes;
using TechChallenge.Oficina.Domain.Features.Insumos;
using TechChallenge.Oficina.Domain.Features.Indicadores;
using TechChallenge.Oficina.Domain.Features.OrdensServico;
using TechChallenge.Oficina.Domain.Features.Servicos;
using TechChallenge.Oficina.Domain.Features.Veiculos;

namespace TechChallenge.Oficina.Infra.Data.Context;

public sealed class OficinaDbContext : DbContext
{
    public OficinaDbContext(DbContextOptions<OficinaDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Indicador> Indicadores => Set<Indicador>();
    public DbSet<Insumo> Insumos => Set<Insumo>();
    public DbSet<ItemServico> ItensServico => Set<ItemServico>();
    public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OficinaDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
