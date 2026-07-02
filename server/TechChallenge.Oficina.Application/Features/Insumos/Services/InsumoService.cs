using AutoMapper;
using TechChallenge.Oficina.Application.Features.Insumos.Commands;
using TechChallenge.Oficina.Application.Features.Insumos.Queries;
using TechChallenge.Oficina.Application.Features.Insumos.ViewModels;
using TechChallenge.Oficina.Domain.Features.Insumos;

namespace TechChallenge.Oficina.Application.Features.Insumos.Services;

public sealed class InsumoService : IInsumoService
{
    private readonly IMapper _mapper;
    private readonly IInsumoRepository _insumoRepository;

    public InsumoService(IMapper mapper, IInsumoRepository insumoRepository)
    {
        _mapper = mapper;
        _insumoRepository = insumoRepository;
    }

    public async Task<InsumoViewModel> CriarAsync(CriarInsumoCommand command, CancellationToken cancellationToken = default)
    {
        var insumo = Insumo.Criar(command.Nome, command.Fabricante, command.QuantidadeDisponivel, command.ValorUnitario);

        await _insumoRepository.AdicionarAsync(insumo, cancellationToken);

        return _mapper.Map<InsumoViewModel>(insumo);
    }

    public async Task<InsumoViewModel> AtualizarAsync(AtualizarInsumoCommand command, CancellationToken cancellationToken = default)
    {
        var insumo = await ObterInsumoExistenteAsync(command.Id, cancellationToken);

        insumo.AtualizarNome(command.Nome);
        insumo.AtualizarFabricante(command.Fabricante);
        insumo.AtualizarQuantidadeDisponivel(command.QuantidadeDisponivel);
        insumo.AtualizarValorUnitario(command.ValorUnitario);

        await _insumoRepository.AtualizarAsync(insumo, cancellationToken);

        return _mapper.Map<InsumoViewModel>(insumo);
    }

    public async Task<InsumoViewModel> ObterPorIdAsync(ObterInsumoPorIdQuery query, CancellationToken cancellationToken = default)
    {
        var insumo = await ObterInsumoExistenteAsync(query.Id, cancellationToken);
        return _mapper.Map<InsumoViewModel>(insumo);
    }

    public async Task<IReadOnlyCollection<InsumoViewModel>> ListarAsync(ListarInsumosQuery query, CancellationToken cancellationToken = default)
    {
        var insumos = await _insumoRepository.ListarAsync(cancellationToken);
        return _mapper.Map<IReadOnlyCollection<InsumoViewModel>>(insumos);
    }

    public async Task ExcluirAsync(ExcluirInsumoCommand command, CancellationToken cancellationToken = default)
    {
        var insumo = await ObterInsumoExistenteAsync(command.Id, cancellationToken);
        await _insumoRepository.RemoverAsync(insumo, cancellationToken);
    }

    private async Task<Insumo> ObterInsumoExistenteAsync(Guid id, CancellationToken cancellationToken)
    {
        var insumo = await _insumoRepository.ObterPorIdAsync(id, cancellationToken);

        if (insumo is null)
        {
            throw new KeyNotFoundException("Insumo não encontrado.");
        }

        return insumo;
    }
}
