using AutoMapper;
using TechChallenge.Oficina.Application.Features.Servicos.Commands;
using TechChallenge.Oficina.Application.Features.Servicos.Queries;
using TechChallenge.Oficina.Application.Features.Servicos.ViewModels;
using TechChallenge.Oficina.Domain.Exceptions;
using TechChallenge.Oficina.Domain.Features.Insumos;
using TechChallenge.Oficina.Domain.Features.Servicos;

namespace TechChallenge.Oficina.Application.Features.Servicos.Services;

public sealed class ServicoService : IServicoService
{
    private readonly IMapper _mapper;
    private readonly IServicoRepository _servicoRepository;
    private readonly IInsumoRepository _insumoRepository;

    public ServicoService(IMapper mapper, IServicoRepository servicoRepository, IInsumoRepository insumoRepository)
    {
        _mapper = mapper;
        _servicoRepository = servicoRepository;
        _insumoRepository = insumoRepository;
    }

    public async Task<ServicoViewModel> CriarAsync(CriarServicoCommand command, CancellationToken cancellationToken = default)
    {
        var itensServico = await ObterItensServicoAsync(command.ItensServico, cancellationToken);
        var servico = Servico.Criar(command.Nome, command.Descricao, itensServico);

        await _servicoRepository.AdicionarAsync(servico, cancellationToken);

        return _mapper.Map<ServicoViewModel>(servico);
    }

    public async Task<ServicoViewModel> AtualizarAsync(AtualizarServicoCommand command, CancellationToken cancellationToken = default)
    {
        var servico = await ObterServicoExistenteAsync(command.Id, cancellationToken);
        var itensServico = await ObterItensServicoAsync(command.ItensServico, cancellationToken);

        servico.AtualizarNome(command.Nome);
        servico.AtualizarDescricao(command.Descricao);
        servico.DefinirItensServico(itensServico);

        await _servicoRepository.AtualizarAsync(servico, cancellationToken);

        return _mapper.Map<ServicoViewModel>(servico);
    }

    public async Task<ServicoViewModel> ObterPorIdAsync(ObterServicoPorIdQuery query, CancellationToken cancellationToken = default)
    {
        var servico = await ObterServicoExistenteAsync(query.Id, cancellationToken);
        return _mapper.Map<ServicoViewModel>(servico);
    }

    public async Task<IReadOnlyCollection<ServicoViewModel>> ListarAsync(ListarServicosQuery query, CancellationToken cancellationToken = default)
    {
        var servicos = await _servicoRepository.ListarAsync(cancellationToken);
        return _mapper.Map<IReadOnlyCollection<ServicoViewModel>>(servicos);
    }

    public async Task ExcluirAsync(ExcluirServicoCommand command, CancellationToken cancellationToken = default)
    {
        var servico = await ObterServicoExistenteAsync(command.Id, cancellationToken);
        await _servicoRepository.RemoverAsync(servico, cancellationToken);
    }

    private async Task<Servico> ObterServicoExistenteAsync(Guid id, CancellationToken cancellationToken)
    {
        var servico = await _servicoRepository.ObterPorIdAsync(id, cancellationToken);

        if (servico is null)
        {
            throw new KeyNotFoundException("Servico nao encontrado.");
        }

        return servico;
    }

    private async Task<IReadOnlyCollection<ItemServico>> ObterItensServicoAsync(IReadOnlyCollection<ItemServicoCommand>? itensServicoCommand, CancellationToken cancellationToken)
    {
        if (itensServicoCommand is null || itensServicoCommand.Count == 0)
        {
            return [];
        }

        if (itensServicoCommand.Any(item => item is null))
        {
            throw new DomainException("Todos os itens de servico informados devem ser validos.");
        }

        if (itensServicoCommand.Any(item => item.InsumoId == Guid.Empty))
        {
            throw new DomainException("Todos os insumos informados devem possuir identificador valido.");
        }

        if (itensServicoCommand.Any(item => item.Quantidade <= 0))
        {
            throw new DomainException("Todos os itens de servico devem possuir quantidade maior que zero.");
        }

        var itensAgrupados = itensServicoCommand
            .GroupBy(item => item.InsumoId)
            .Select(group => new { InsumoId = group.Key, Quantidade = group.Sum(item => item.Quantidade) })
            .ToArray();

        var itensServico = new List<ItemServico>(itensAgrupados.Length);

        foreach (var item in itensAgrupados)
        {
            var insumo = await _insumoRepository.ObterPorIdAsync(item.InsumoId, cancellationToken);

            if (insumo is null)
            {
                throw new KeyNotFoundException("Insumo nao encontrado.");
            }

            itensServico.Add(ItemServico.Criar(insumo, item.Quantidade));
        }

        return itensServico;
    }
}
