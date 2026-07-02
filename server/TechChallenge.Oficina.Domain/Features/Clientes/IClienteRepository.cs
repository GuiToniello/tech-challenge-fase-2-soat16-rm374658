namespace TechChallenge.Oficina.Domain.Features.Clientes;

public interface IClienteRepository
{
    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Cliente>> ListarAsync(CancellationToken cancellationToken = default);
    Task<bool> ExisteComIdentificacaoAsync(string identificacaoNormalizada, Guid? ignorarClienteId = null, CancellationToken cancellationToken = default);
    Task RemoverAsync(Cliente cliente, CancellationToken cancellationToken = default);
}
