namespace TechChallenge.Oficina.Application.Features.OrdensServico.Commands;

public sealed class AtualizarOrdemServicoCommand
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public IReadOnlyCollection<Guid> ServicoIds { get; set; } = [];
}
