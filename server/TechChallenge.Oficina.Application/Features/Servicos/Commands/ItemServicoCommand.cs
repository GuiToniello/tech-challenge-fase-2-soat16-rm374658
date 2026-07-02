namespace TechChallenge.Oficina.Application.Features.Servicos.Commands;

public sealed class ItemServicoCommand
{
    public Guid InsumoId { get; set; }
    public int Quantidade { get; set; }
}
