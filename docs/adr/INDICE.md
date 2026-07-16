# ADRs - Architecture Decision Records

## Índice

1. [ADR-001: Clean Architecture com Folder-by-Feature](ADR-001-Clean-Architecture.md)
2. [ADR-002: Domain-Driven Design e Entidades Ricas](ADR-002-DDD-e-Entidades-Ricas.md)
3. [ADR-003: Value Objects para Validação de Identificação (CPF/CNPJ)](ADR-003-Value-Objects-Identificacao.md)
4. [ADR-004: Gateways e Persistência](ADR-004-Gateways-e-Persistencia.md)
5. [ADR-005: Separação de Contratos em Commands/Queries/ViewModels](ADR-005-Separacao-Commands-Queries-ViewModels.md)
6. [ADR-006: Mapeamento com AutoMapper nos UseCases](ADR-006-AutoMapper-nos-UseCases.md)
7. [ADR-007: Entity Framework Core com PostgreSQL](ADR-007-EFCore-com-PostgreSQL.md)
8. [ADR-008: Swagger para Documentação de API](ADR-008-Swagger-para-API.md)
9. [ADR-009: Migrações Automáticas no Startup](ADR-009-Migracoes-Automaticas-no-Startup.md)
10. [ADR-011: Envio de E-mail de Orçamento com Resend](ADR-011-Envio-de-Email-de-Orcamento.md)
11. [ADR-012: Controle de Estoque de Insumos](ADR-012-Controle-de-Estoque-de-Insumos.md)

## Resumo de Contextos

### Contexto de Arquitetura
- **ADR-001**: Clean Architecture com folder-by-feature e regra de dependência
- **ADR-004**: Gateways como portas de persistência no projeto UseCases com implementação no projeto DB

### Contexto de Domain-Driven Design
- **ADR-002**: Entidades ricas com comportamento validador
- **ADR-003**: Value Objects para validação de identificação

### Contexto de Separação de Responsabilidades
- **ADR-005**: Contratos segregados (Commands/Queries/ViewModels)
- **ADR-006**: AutoMapper centralizado no projeto UseCases

### Contexto de Infraestrutura de Dados
- **ADR-007**: EF Core + PostgreSQL com migrations versionadas
- **ADR-009**: Migrações automáticas no startup

### Contexto de Exposição da API
- **ADR-008**: Swagger para documentação auto-atualizada

### Contexto de Comunicação com Cliente
- **ADR-011**: Envio de orçamento por e-mail via Resend com contrato no domínio

### Contexto de Controle de Estoque
- **ADR-012**: Verificação e débito de estoque de insumos em ordens de serviço

