using Microsoft.EntityFrameworkCore;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;
using TechChallenge.Oficina.DB.Data.Context;

namespace TechChallenge.Oficina.DB.Data.Features.OrdensServico;

public sealed class OrdemServicoRepository : IOrdemServicoRepository
{
    private readonly OficinaDbContext _context;

    public OrdemServicoRepository(OficinaDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        await _context.OrdensServico.AddAsync(ordemServico, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        _context.OrdensServico.Update(ordemServico);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrdemServico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await CriarConsultaCompleta()
            .FirstOrDefaultAsync(ordemServico => ordemServico.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrdemServico>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await CriarConsultaCompleta()
            .OrderBy(ordemServico => ordemServico.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrdemServico>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return await CriarConsultaCompleta()
            .Where(ordemServico => ordemServico.ClienteId == clienteId)
            .OrderBy(ordemServico => ordemServico.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrdemServico>> ListarPorStatusAsync(StatusOrdemServico status, CancellationToken cancellationToken = default)
    {
        return await CriarConsultaCompleta()
            .Where(ordemServico => ordemServico.Status == status)
            .OrderBy(ordemServico => ordemServico.Id)
            .ToArrayAsync(cancellationToken);
    }

    public async Task RemoverAsync(OrdemServico ordemServico, CancellationToken cancellationToken = default)
    {
        _context.OrdensServico.Remove(ordemServico);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<OrdemServico> CriarConsultaCompleta()
    {
        return _context.OrdensServico
            .Include(ordemServico => ordemServico.Servicos)
                .ThenInclude(servico => servico.ItensServico)
                .ThenInclude(itemServico => itemServico.Insumo)
            .Include(ordemServico => ordemServico.Orcamento)
                .ThenInclude(orcamento => orcamento!.Servicos)
            .Include(ordemServico => ordemServico.HistoricoStatus);
    }
}
