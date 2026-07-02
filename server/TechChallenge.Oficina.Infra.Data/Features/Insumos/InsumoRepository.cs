using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Domain.Features.Insumos;
using TechChallenge.Oficina.Infra.Data.Context;

namespace TechChallenge.Oficina.Infra.Data.Features.Insumos;

public sealed class InsumoRepository : IInsumoRepository
{
    private readonly OficinaDbContext _context;

    public InsumoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(Insumo insumo, CancellationToken cancellationToken = default)
    {
        await _context.Insumos.AddAsync(insumo, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Insumo insumo, CancellationToken cancellationToken = default)
    {
        _context.Insumos.Update(insumo);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Insumo?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Insumos
            .FirstOrDefaultAsync(insumo => insumo.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Insumo>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Insumos
            .OrderBy(insumo => insumo.Nome)
            .ThenBy(insumo => insumo.Fabricante)
            .ToArrayAsync(cancellationToken);
    }

    public async Task RemoverAsync(Insumo insumo, CancellationToken cancellationToken = default)
    {
        _context.Insumos.Remove(insumo);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
