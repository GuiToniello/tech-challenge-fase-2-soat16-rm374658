using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Domain.Features.Indicadores;
using TechChallenge.Oficina.Infra.Data.Context;

namespace TechChallenge.Oficina.Infra.Data.Features.Indicadores;

public sealed class IndicadorRepository : IIndicadorRepository
{
    private readonly OficinaDbContext _context;

    public IndicadorRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task<Indicador?> ObterAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Indicadores.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SalvarAsync(Indicador indicador, CancellationToken cancellationToken = default)
    {
        var existente = await _context.Indicadores.FirstOrDefaultAsync(cancellationToken);

        if (existente is null)
        {
            await _context.Indicadores.AddAsync(indicador, cancellationToken);
        }
        else
        {
            existente.Atualizar(indicador.TempoMedioExecucao, indicador.TempoMedioEntrega);
            _context.Indicadores.Update(existente);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
