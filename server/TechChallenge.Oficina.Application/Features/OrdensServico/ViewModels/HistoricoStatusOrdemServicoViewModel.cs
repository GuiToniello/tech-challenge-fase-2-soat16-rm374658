using TechChallenge.Oficina.Domain.Features.OrdensServico.Enums;

namespace TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;

public sealed class HistoricoStatusOrdemServicoViewModel
{
    public StatusOrdemServico Status { get; set; }
    public DateTime DataAlteracao { get; set; }
}
