# Projeto Oficina

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=GuiToniello_tech-challenge-fase-1-soat16-rm374658&metric=alert_status&token=ea0031cd24511d30496ed0d47a909e4881b37946)](https://sonarcloud.io/summary/new_code?id=GuiToniello_tech-challenge-fase-1-soat16-rm374658) [![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=GuiToniello_tech-challenge-fase-1-soat16-rm374658&token=ea0031cd24511d30496ed0d47a909e4881b37946)](https://sonarcloud.io/summary/new_code?id=GuiToniello_tech-challenge-fase-1-soat16-rm374658) [![SonarQube Cloud](https://sonarcloud.io/images/project_badges/sonarcloud-light.svg)](https://sonarcloud.io/summary/new_code?id=GuiToniello_tech-challenge-fase-1-soat16-rm374658)

## 1. Identificacao

Software de gestão para uma oficina mecânica.

Tech Challenge da Fase 2 do curso SOAT16 da FIAP.

Grupo:
-  Guilherme Toniello Vieira -  SOAT16 - rm374658

### 1.2 Links Uteis

Acesse o blueprint no Miro: https://miro.com/app/board/uXjVHVfHuvI=/?share_link_id=633470424823

O link do sonar: https://sonarcloud.io/summary/overall?id=GuiToniello_tech-challenge-fase-1-soat16-rm374658

Você pode acessar o relatório completo [aqui](./reports/relatorio-completo.pdf).

## 2. Arquitetura

Nessa seção, será descrita a arquitetura em alto nível e organização da solução.

### 2.1. Visao Geral

As decisões arquiteturais estão documentadas individualmente como ADRs (Architecture Decision Records) em `docs/adr/`:

| ADR | Resumo | Link |
|-----|--------|------|
| ADR-001 — Clean Architecture com Folder-by-Feature | Adota Clean Architecture com 4 anéis de dependência (Entities, UseCases, Controllers, API/Infra), organizados por feature. Define separação de responsabilidades guiada pela regra de inversão de dependência. | [ver](./docs/adr/ADR-001-Clean-Architecture.md) |
| ADR-002 — DDD e Entidades Ricas | Entidades encapsulam comportamentos e validações de domínio via factory methods obrigatórios. Evita entidades anêmicas; erros de negócio são sinalizados com `DomainException`. | [ver](./docs/adr/ADR-002-DDD-e-Entidades-Ricas.md) |
| ADR-003 — Value Objects para CPF/CNPJ | CPF e CNPJ são encapsulados em Value Objects com validação de dígitos verificadores. `IdentificacaoCliente` detecta o tipo automaticamente e garante que apenas valores válidos existam no domínio. | [ver](./docs/adr/ADR-003-Value-Objects-Identificacao.md) |
| ADR-004 — Gateways e Persistência | Contratos de persistência (Gateways) residem nos UseCases; implementações ficam no projeto DB via EF Core. Leituras retornam coleções materializadas; escritas persistem via `SaveChangesAsync`. | [ver](./docs/adr/ADR-004-Gateways-e-Persistencia.md) |
| ADR-005 — Separação de Commands/Queries/ViewModels | Commands (escrita), Queries (leitura) e ViewModels (resposta) são contratos explícitos na camada de UseCases. A API consome esses contratos diretamente, sem criar os seus próprios. | [ver](./docs/adr/ADR-005-Separacao-Commands-Queries-ViewModels.md) |
| ADR-006 — AutoMapper nos UseCases | Mapeamento entre entidades de domínio e DTOs é responsabilidade exclusiva dos UseCases via AutoMapper. Controllers nunca instanciam `IMapper`; recebem apenas ViewModels já mapeadas. | [ver](./docs/adr/ADR-006-AutoMapper-nos-UseCases.md) |
| ADR-007 — EF Core com PostgreSQL | EF Core 10.x com Npgsql como ORM para PostgreSQL, com migrations versionadas. Configurações de mapeamento via `IEntityTypeConfiguration<T>` e connection string via `appsettings.json`. | [ver](./docs/adr/ADR-007-EFCore-com-PostgreSQL.md) |
| ADR-008 — Swagger para Documentação | Documentação automática de endpoints via Swashbuckle com suporte a autenticação JWT no UI. Swagger UI disponível na raiz da aplicação. | [ver](./docs/adr/ADR-008-Swagger-para-API.md) |
| ADR-009 — Migrações Automáticas no Startup | Migrations pendentes são aplicadas automaticamente ao iniciar a API via `ApplyMigrations()`. Elimina intervenção manual no deploy, incluindo ambientes de container. | [ver](./docs/adr/ADR-009-Migracoes-Automaticas-no-Startup.md) |
| ADR-011 — Envio de E-mail de Orçamento | Notifica o cliente por e-mail ao gerar orçamento via porta de saída `IOrcamentoEmailSender` e provedor Resend. A API funciona normalmente sem a ApiKey; o envio de e-mail é desabilitado. | [ver](./docs/adr/ADR-011-Envio-de-Email-de-Orcamento.md) |
| ADR-012 — Controle de Estoque de Insumos | Verifica estoque ao gerar orçamento (sem reserva) e debita automaticamente ao aprovar. Lança `DomainException` se houver estoque insuficiente em qualquer etapa. | [ver](./docs/adr/ADR-012-Controle-de-Estoque-de-Insumos.md) |
| ADR-013 — Tratamento de Erros em Duas Camadas | Erros de negócio esperados são tratados nos controllers com resposta HTTP semântica (4xx). Erros inesperados são capturados pelo middleware global, evitando vazamento de stack trace. | [ver](./docs/adr/ADR-013-Estrategia-de-Tratamento-de-Erros-em-Duas-Camadas.md) |

### 2.2. Autenticacao

O projeto usa autenticação via JWT emitido pelo `Auth0` (https://auth0.com/) como Identity Provider.

**Como funciona na prática:**

A API valida cada requisição verificando o JWT no header `Authorization: Bearer <token>`. A integração usa o pacote `Microsoft.AspNetCore.Authentication.JwtBearer`. As configurações ficam em `appsettings.json` na chave `AuthSettings`:

```json
"AuthSettings": {
  "Authority": "<seu-dominio>.us.auth0.com",
  "Audience": "https://localhost:7194"
}
```

O `Authority` aponta para o domínio Auth0, que expõe o endpoint `/.well-known/openid-configuration` usado pelo middleware para buscar as chaves públicas de validação. O `Audience` identifica esta API junto ao Auth0.

Todos os endpoints exigem autenticação por padrão (política de fallback `RequireAuthenticatedUser`). O único endpoint público é `/health`.

**Você não precisa criar uma conta no Auth0.** A collection do Postman em `/e2e` já possui um request configurado com as credenciais de demonstração para obter o token de acesso via Client Credentials Flow.

### 2.3. CQRS (CQS)

O padrão implementado é o **CQS (Command Query Separation)** — o nível mais básico do CQRS — sem separação de banco de dados para leitura e escrita.

A separação acontece na **camada de UseCases** (`3 - Application Business Rules`). Cada feature é organizada com as seguintes pastas:

- `Commands/` — objetos de entrada para operações de escrita (ex: `CriarClienteCommand`, `AtualizarClienteCommand`)
- `Queries/` — objetos de entrada para operações de leitura (ex: `ListarClientesQuery`, `ObterClientePorIdQuery`)
- `ViewModels/` — objetos de saída (resposta da API)
- `UseCases/` — a classe que executa a operação, recebendo um `Command` ou uma `Query` como parâmetro tipado

O benefício é tornar a **intenção explícita no contrato**: ao receber um `Command`, sabe-se que haverá efeito colateral; ao receber uma `Query`, sabe-se que é apenas leitura. Não há mediator (ex: MediatR), o roteamento é feito diretamente via injeção de dependência.

## 3. Referencias Bibliograficas

### 3.1. Padroes de Arquitetura

Esse projeto foi construído usando os padrões descritos abaixo.

- Clean Architecture (4 aneis e regra de dependencia)
  - ADR: [ADR-001](./docs/adr/ADR-001-Clean-Architecture.md)
  - Codigo: [server/](./server/)
- Domain-Driven Design (DDD)
  - ADR: [ADR-002](./docs/adr/ADR-002-DDD-e-Entidades-Ricas.md)
  - Codigo: [Entidades de dominio](./server/4%20-%20Enterprise%20Business%20Rules/TechChallenge.Oficina.Entities/)
- CQS (separacao entre Commands e Queries)
  - ADR: [ADR-005](./docs/adr/ADR-005-Separacao-Commands-Queries-ViewModels.md)
  - Codigo: [UseCases](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/)

### 3.2. Padroes Estruturais

- Folder-by-Feature
  - ADR: [ADR-001](./docs/adr/ADR-001-Clean-Architecture.md)
- Gateway/Repository para persistencia
  - ADR: [ADR-004](./docs/adr/ADR-004-Gateways-e-Persistencia.md)
  - Codigo: [Gateways DB](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.DB/)
- Mapper/Adapter (AutoMapper nos UseCases)
  - ADR: [ADR-006](./docs/adr/ADR-006-AutoMapper-nos-UseCases.md)
  - Codigo: [UseCases](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/)
- Dependency Injection (Inversion of Control)
  - Codigo: [Program.cs](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.API/Program.cs)
- Service Layer (aplicada na camada de UseCases)
  - Codigo: [UseCases](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/)

### 3.3. Padroes de Design

- Factory Method (entidades ricas com metodos de criacao)
  - ADR: [ADR-002](./docs/adr/ADR-002-DDD-e-Entidades-Ricas.md)
- Value Object (CPF/CNPJ e identificacao)
  - ADR: [ADR-003](./docs/adr/ADR-003-Value-Objects-Identificacao.md)
- DTO/ViewModel Pattern
  - ADR: [ADR-005](./docs/adr/ADR-005-Separacao-Commands-Queries-ViewModels.md)
- Centralized Exception Handling (duas camadas: controller + middleware)
  - ADR: [ADR-013](./docs/adr/ADR-013-Estrategia-de-Tratamento-de-Erros-em-Duas-Camadas.md)
  - Codigo: [Middleware](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.API/Middleware/) e [Controllers](./server/2%20-%20Interface%20Adapters/TechChallenge.Oficina.Controllers/)

### 3.4. Padroes Comportamentais

- CQS (Command Query Separation)
  - ADR: [ADR-005](./docs/adr/ADR-005-Separacao-Commands-Queries-ViewModels.md)
  - Implementacao: Commands (escrita), Queries (leitura), ViewModels (resposta)
- Domain Exceptions para regras de negocio e invariantes
  - ADR: [ADR-002](./docs/adr/ADR-002-DDD-e-Entidades-Ricas.md)
  - Codigo: [Exceptions](./server/4%20-%20Enterprise%20Business%20Rules/TechChallenge.Oficina.Entities/Exceptions/)

### 3.5. Referencias

#### 3.5.1. Livros e Artigos

MARTIN, Robert C. Clean Architecture: A Craftsman's Guide to Software Structure and Design. Boston: Prentice Hall, 2017.

EVANS, Eric. Domain-Driven Design: Tackling Complexity in the Heart of Software. Boston: Addison-Wesley, 2003.

VERNON, Vaughn. Implementing Domain-Driven Design. Boston: Addison-Wesley, 2013.

FOWLER, Martin. Patterns of Enterprise Application Architecture. Boston: Addison-Wesley, 2003.

GAMMA, Erich; HELM, Richard; JOHNSON, Ralph; VLISSIDES, John. Design Patterns: Elements of Reusable Object-Oriented Software. Boston: Addison-Wesley, 1994.

MARTIN, Robert C. Agile Software Development, Principles, Patterns, and Practices. Upper Saddle River: Prentice Hall, 2002.

#### 3.5.2. Material de Apoio

FOWLER, Martin. CQRS. Martin Fowler, 14 jul. 2011. Disponível em: https://martinfowler.com/bliki/CQRS.html. Acesso em: 10 jun. 2026.

FOWLER, Martin. Inversion of Control Containers and the Dependency Injection pattern. Martin Fowler, 23 jan. 2004. Disponível em: https://martinfowler.com/articles/injection.html. Acesso em: 02 jun. 2026.

## 4. Executando o Projeto

Para executar o projeto, temos 3 alternativas descritas nas subseções `4.2`, `4.3` e `4.4`.

Siga apenas 1 delas.

### 4.1. Pre-requisito

Se você for rodar sem container, vai precisar:

- dotnet 10.x
https://dotnet.microsoft.com/pt-br/download/dotnet/thank-you/sdk-10.0.301-windows-x64-installer

- Postgres instalado e com instancia ativa: https://www.postgresql.org/

Para containers, precisa do docker (https://www.docker.com/) ou Podman (https://podman.io/) instalado.

Já temos docker-compose pronto com todas as configurações.

> Recomenda-se o uso de containers

### 4.2. Alternativa A - Containers (docker, podman, ...)

Passo 1 - Com o console apontado para o root do repositório, execute `docker-compose up -d -b`

Se estiver usando o podman, use `podman compose up -d --build`

E pronto!

o banco de dados `postgres` e a `api` estarão disponíveis.

Passo 2 - Use `http://localhost:8080/index.html` para acessar o swagger.

### 4.3. Alternativa B - Local com dotnet cli

Passo 1 - rode o `postgres` - pode ser uma instancia local ou via container `docker-compose run postgres -d`.

Passo 2 - aponte o console para a pasta `server/1 - Frameworks & Drivers/TechChallenge.Oficina.API/` e então execute `dotnet run`

Pronto!

Vai subir a API usando https com um certificado autoassinado do dotnet.

Passo 3 - Use `https://localhost:7194/index.html` para acessar o Swagger.

### 4.4. Alternativa C - Local com visual studio 2026 ou vs code

Passo 1 - rode o `postgres` - pode ser uma instancia local ou via container `docker-compose run postgres -d`.

Passo 2 - abra o arquivo `.slnx` em `/server`

Passo 3 - no visual studio, rode usando o perfil `https`.

Passo 4 - Use `https://localhost:7194/index.html` para acessar o Swagger.


### 4.5  Banco de dados

Para popular o banco de dados, use a collection do postman na pasta `/e2e`.

Não temos scripts SQL ou endpoint, apenas use a collection que ela irá popular o banco e executar demais operações de demonstração.

Se você estiver rodando em container, não é preciso configurar mais nada, apenas rodar, a connection string já está certa.

Se você está rodando o postgres localmente, com instancia nao-container, precisa configurar a connection string em `server/1 - Frameworks & Drivers/TechChallenge.Oficina.API/appsettings.json`, na chave `DatabaseSettings:ConnectionString`.

## 5. Requisitos Implementados

| Requisito | Atende? | Observação de escopo | Evidências (server/) |
|---|---|---|---|
| Consulta de status da Ordem de Serviço (OS) | Sim | Consulta de status disponível no fluxo de OS. | [Endpoints OS](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.API/Features/OrdensServico/OrdensServicoEndpoints.cs) · [Controller OS](./server/2%20-%20Interface%20Adapters/TechChallenge.Oficina.Controllers/Features/OrdensServico/OrdensServicoController.cs) |
| Aprovação/recusa de orçamento por endpoint externo | Sim | Aprovação e recusa implementadas no fluxo da OS. | [Endpoints OS](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.API/Features/OrdensServico/OrdensServicoEndpoints.cs) · [UseCases OS](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/Features/OrdensServico/UseCases/OrdemServicoUseCases.cs) |
| Abertura de OS com cliente, veículo, serviços e peças, retornando ID único | Sim | Abertura completa com retorno da identificação da OS. | [Controller OS](./server/2%20-%20Interface%20Adapters/TechChallenge.Oficina.Controllers/Features/OrdensServico/OrdensServicoController.cs) · [ViewModel Abertura](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/Features/OrdensServico/ViewModels/AberturaOrdemServicoViewModel.cs) |
| Listagem de OS por prioridade + antiguidade, excluindo finalizadas/entregues | Sim | Escopo acordado: endpoint ordenado /ordenadas. | [Gateway OS](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.DB/Features/OrdensServico/OrdemServicoGateway.cs) · [Query Ordenada](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/Features/OrdensServico/Queries/ListarOrdensServicoOrdenadasQuery.cs) |
| Atualização de status da OS com notificação ao cliente | Sim | Escopo acordado: mudanças de status da OS; orçamento separado em /enviar-orçamento. | [UseCases OS](./server/3%20-%20Application%20Business%20Rules/TechChallenge.Oficina.UseCases/Features/OrdensServico/UseCases/OrdemServicoUseCases.cs) · [Sender Status](./server/1%20-%20Frameworks%20%26%20Drivers/TechChallenge.Oficina.Email/Features/OrdensServico/OrdemServicoStatusEmailSender.cs) |

## 6. Finalizacao

- Para fazer requisições, use a `collection` do `postman` na pasta `/e2e`
- Para o envio de emails, é preciso configurar `ApiKey` no appsettings.json ou `ResendSettings__ApiKey` para container

Você pode logar um github em https://resend.com/, criar sua conta e gerar a apiKey.

Sem a apiKey, a API funciona normal, só nao envia os emails.
