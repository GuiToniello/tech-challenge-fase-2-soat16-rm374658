using TechChallenge.Oficina.Entities.Features.Insumos;
using TechChallenge.Oficina.Entities.Features.Servicos;

namespace TechChallenge.Oficina.UseCases.Features.Insumos.UseCases;

public sealed class EstoqueUseCases : IEstoqueUseCases
{
    private readonly IInsumoRepository _insumoRepository;

    public EstoqueUseCases(IInsumoRepository insumoRepository)
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
