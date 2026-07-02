namespace TechChallenge.Oficina.Application.Features.OrdensServico.ViewModels;

public sealed class OrcamentoViewModel
{
    public Guid OrdemServicoId { get; set; }
    public DateTime DataGeracao { get; set; }
    public decimal ValorTotal { get; set; }
    public IReadOnlyCollection<OrcamentoServicoViewModel> Servicos { get; set; } = [];
}
