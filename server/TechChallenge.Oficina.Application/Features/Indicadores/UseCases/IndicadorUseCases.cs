using AutoMapper;
using TechChallenge.Oficina.UseCases.Features.Indicadores.Queries;
using TechChallenge.Oficina.UseCases.Features.Indicadores.ViewModels;
using TechChallenge.Oficina.Entities.Features.Indicadores;
using TechChallenge.Oficina.Entities.Features.OrdensServico;
using TechChallenge.Oficina.Entities.Features.OrdensServico.Enums;
using TechChallenge.Oficina.UseCases.Features.Indicadores.UseCases;
using TechChallenge.Oficina.UseCases.Features.OrdensServico.UseCases;

namespace TechChallenge.Oficina.UseCases.Features.Indicadores.Services;

public sealed class IndicadorUseCases : IIndicadorUseCases
{
    private readonly IIndicadorRepository _indicadorRepository;
    private readonly IOrdemServicoRepository _ordemServicoRepository;

    public IndicadorUseCases(IIndicadorRepository indicadorRepository, IOrdemServicoRepository ordemServicoRepository)
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
