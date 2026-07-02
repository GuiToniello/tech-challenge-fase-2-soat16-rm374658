using AutoMapper;
using TechChallenge.Oficina.Application.Features.Indicadores.Queries;
using TechChallenge.Oficina.Application.Features.Indicadores.ViewModels;
using TechChallenge.Oficina.Domain.Features.Indicadores;
using TechChallenge.Oficina.Domain.Features.OrdensServico;
using TechChallenge.Oficina.Domain.Features.OrdensServico.Enums;

namespace TechChallenge.Oficina.Application.Features.Indicadores.Services;

public sealed class IndicadorService : IIndicadorService
{
    private readonly IIndicadorRepository _indicadorRepository;
    private readonly IOrdemServicoRepository _ordemServicoRepository;

    public IndicadorService(IIndicadorRepository indicadorRepository, IOrdemServicoRepository ordemServicoRepository)
    {
        _indicadorRepository = indicadorRepository;
        _ordemServicoRepository = ordemServicoRepository;
    }

    public async Task<IndicadorViewModel> ObterAsync(ObterIndicadoresQuery query, CancellationToken cancellationToken = default)
    {
        var indicador = await _indicadorRepository.ObterAsync(cancellationToken);

        if (indicador is null)
        {
            return new IndicadorViewModel();
        }

        return new IndicadorViewModel
        {
            TempoMedioExecucao = indicador.TempoMedioExecucao,
            TempoMedioEntrega = indicador.TempoMedioEntrega
        };
    }

    public async Task AtualizarAsync(CancellationToken cancellationToken = default)
    {
        var ordensEntregues = await _ordemServicoRepository.ListarPorStatusAsync(StatusOrdemServico.Entregue, cancellationToken);
        var temposExecucao = ordensEntregues.Select(ordemServico => ordemServico.ObterTempoExecucao()).ToArray();
        var temposEntrega = ordensEntregues.Select(ordemServico => ordemServico.ObterTempoEntrega()).ToArray();

        var indicador = await _indicadorRepository.ObterAsync(cancellationToken) ?? Indicador.Criar(TimeSpan.Zero, TimeSpan.Zero);
        indicador.Atualizar(CalcularMedia(temposExecucao), CalcularMedia(temposEntrega));
        await _indicadorRepository.SalvarAsync(indicador, cancellationToken);
    }

    private static TimeSpan CalcularMedia(TimeSpan[] intervalos)
    {
        if (intervalos.Length == 0)
        {
            return TimeSpan.Zero;
        }

        var mediaTicks = intervalos.Average(intervalo => intervalo.Ticks);
        return TimeSpan.FromTicks(Convert.ToInt64(mediaTicks));
    }
}
