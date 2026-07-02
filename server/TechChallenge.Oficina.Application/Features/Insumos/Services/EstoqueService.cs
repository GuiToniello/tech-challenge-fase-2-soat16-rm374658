using TechChallenge.Oficina.Domain.Features.Insumos;
using TechChallenge.Oficina.Domain.Features.Servicos;

namespace TechChallenge.Oficina.Application.Features.Insumos.Services;

public sealed class EstoqueService : IEstoqueService
{
    private readonly IInsumoRepository _insumoRepository;

    public EstoqueService(IInsumoRepository insumoRepository)
    {
        _insumoRepository = insumoRepository;
    }

    public async Task VerificarDisponibilidadeParaOrcamentoAsync(IReadOnlyCollection<Servico> servicos, CancellationToken cancellationToken = default)
    {
        var insumosNecessarios = AgruparInsumosPorId(servicos);

        foreach (var (insumoId, quantidadeTotal) in insumosNecessarios)
        {
            var insumo = await _insumoRepository.ObterPorIdAsync(insumoId, cancellationToken);

            if (insumo is null)
            {
                throw new KeyNotFoundException($"Insumo com ID '{insumoId}' nao encontrado.");
            }

            insumo.VerificarDisponibilidade(quantidadeTotal);
        }
    }

    public async Task DebitarEstoqueParaOrdemServicoAsync(IReadOnlyCollection<Servico> servicos, CancellationToken cancellationToken = default)
    {
        var insumosNecessarios = AgruparInsumosPorId(servicos);

        foreach (var (insumoId, quantidadeTotal) in insumosNecessarios)
        {
            var insumo = await _insumoRepository.ObterPorIdAsync(insumoId, cancellationToken);

            if (insumo is null)
            {
                throw new KeyNotFoundException($"Insumo com ID '{insumoId}' nao encontrado.");
            }

            insumo.DebitarEstoque(quantidadeTotal);
            await _insumoRepository.AtualizarAsync(insumo, cancellationToken);
        }
    }

    private static Dictionary<Guid, int> AgruparInsumosPorId(IReadOnlyCollection<Servico> servicos)
    {
        var insumosAgrupados = new Dictionary<Guid, int>();

        foreach (var servico in servicos)
        {
            foreach (var itemServico in servico.ItensServico)
            {
                if (insumosAgrupados.ContainsKey(itemServico.InsumoId))
                {
                    insumosAgrupados[itemServico.InsumoId] += itemServico.Quantidade;
                }
                else
                {
                    insumosAgrupados[itemServico.InsumoId] = itemServico.Quantidade;
                }
            }
        }

        return insumosAgrupados;
    }
}
