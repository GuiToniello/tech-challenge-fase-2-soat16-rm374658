# ADR-004: Repository Pattern e Persistência

**Status**: Aceita

**Contexto**:
Necessidade de abstrair a lógica de persistência, permitir múltiplas implementações (testes, diferentes BDs), e manter a camada de dados isolada do domínio.

**Decisão**:
- Interface `IClienteRepository` fica no **Domain**.
- Implementação `ClienteRepository` fica em **Infra.Data**.
- Métodos do repositório (`AdicionarAsync`, `AtualizarAsync`, `RemoverAsync`) são responsáveis por persistência, incluindo `SaveChangesAsync()`.
- Repositório não retorna `IQueryable` (evita leaky abstractions).
- Métodos de busca retornam `IReadOnlyCollection<>` para proteger coleções.

**Consequências**:
- ✅ Fácil mockar para testes.
- ✅ Centraliza lógica de persistência.
- ✅ Protege estado da coleção retornada.
- ✅ SaveChanges encapsulado previne esquecimento de persist.

**Assinatura**:
```csharp
public interface IClienteRepository
{
    Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task RemoverAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Cliente>> ListarAsync(CancellationToken cancellationToken = default);
    Task<bool> ExisteComIdentificacaoAsync(string identificacaoNormalizada, Guid? ignorarClienteId = null, CancellationToken cancellationToken = default);
}
```
