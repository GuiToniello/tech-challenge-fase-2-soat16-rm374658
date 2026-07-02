# ADR-001: Arquitetura em Camadas (N-Layered) com Folder-by-Feature

**Status**: Aceita

**Contexto**:
Precisamos de organizar um backend de serviço de oficina com múltiplas entidades mantendo separação de responsabilidades clara.

**Decisão**:
Adotar arquitetura em camadas com cinco projetos principais:
- **Domain**: Lógica de negócio, entidades ricas, Value Objects, interfaces de repositório, exceções de domínio.
- **Application**: Orquestração de fluxos, serviços, mapeamento (AutoMapper), commands/queries/viewmodels.
- **Infra.Data**: Implementação de repositórios, contexto EF Core, configurações de entidades, migrations.
- **Infra**: Configurações e utilidades.
- **API**: Controllers, dependências externas, configuração do Swagger.

Cada camada organizada por **folder-by-feature** (ex: `Features/Clientes/...`).

**Consequências**:
- Separação clara de responsabilidades.
- Facilita testes unitários isolados por camada.
- Escalabilidade para novos agregados.
- Possível over-engineering em aplicações muito pequenas.
