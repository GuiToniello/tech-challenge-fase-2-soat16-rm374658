# Projeto Oficina

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=GuiToniello_tech-challenge-fase-1-soat16-rm374658&metric=alert_status&token=ea0031cd24511d30496ed0d47a909e4881b37946)](https://sonarcloud.io/summary/new_code?id=GuiToniello_tech-challenge-fase-1-soat16-rm374658) [![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=GuiToniello_tech-challenge-fase-1-soat16-rm374658&token=ea0031cd24511d30496ed0d47a909e4881b37946)](https://sonarcloud.io/summary/new_code?id=GuiToniello_tech-challenge-fase-1-soat16-rm374658) [![SonarQube Cloud](https://sonarcloud.io/images/project_badges/sonarcloud-light.svg)](https://sonarcloud.io/summary/new_code?id=GuiToniello_tech-challenge-fase-1-soat16-rm374658)

Software de gestão para uma oficina mecânica.

Tech Challenge da Fase 2 do curso SOAT16 da FIAP.

Grupo:
-  Guilherme Toniello Vieira -  SOAT16 - rm374658


> Acesse o blueprint no Miro: https://miro.com/app/board/uXjVHVfHuvI=/?share_link_id=633470424823

O link do sonar: https://sonarcloud.io/summary/overall?id=GuiToniello_tech-challenge-fase-1-soat16-rm374658


Você pode acessar o relatório completo [aqui](./reports/relatorio-completo.pdf).

## Arquitetura

Esse projeto foi construído usando os padrões descritos abaixo.

Padrões de Arquitetura

- Layered Architecture
- Domain Driven Design

Padrões Estruturais

- Folder-by-Feature 
- Repository Pattern
- Facade Pattern
- Mapper / Adapter Pattern
- Dependency Injection 
- Service Layer 

Padrões de Design

- Factory Method Pattern
- DTO Pattern
- Centralized Exception Handling

Padrões Comportamentais

- CQRS (Command Query Responsibility Segregation)
    - Feito o nível mais básico: separação da intenção (CQS)
    - Commands (escrita), Queries (leitura), ViewModels (resposta)

### Referências

As referências abaixo foram usadas como base para a implementação dos padrões descritos acima.

#### Livros e Artigos

Buschmann, F., Meunier, R., Rohnert, H., Sommerlad, P., & Stal, M. (1996). Pattern-Oriented Software Architecture: A System of Patterns.

FOWLER, Martin. Patterns of Enterprise Application Architecture. Boston: Addison-Wesley, 2003.

GAMMA, Erich; HELM, Richard; JOHNSON, Ralph; VLISSIDES, John. Design Patterns: Elements of Reusable Object-Oriented Software. Boston: Addison-Wesley, 1994.

MARTIN, Robert C. Agile Software Development, Principles, Patterns, and Practices. Upper Saddle River: Prentice Hall, 2002.

WIRFS-BROCK, Rebecca J. Toward Exception-Handling Best Practices and Patterns. IEEE Software, v. 23, n. 5, p. 11-13, 2006. DOI: 10.1109/MS.2006.129.

#### Material de Apoio

FOWLER, Martin. CQRS. Martin Fowler, 14 jul. 2011. Disponível em: https://martinfowler.com/bliki/CQRS.html. Acesso em: 10 jun. 2026.

STAFFORD, Randy. Service Layer. Martin Fowler, 5 mar. 2003. Disponível em: https://martinfowler.com/eaaCatalog/serviceLayer.html. Acesso em: 12 jun. 2026.

FOWLER, Martin. Inversion of Control Containers and the Dependency Injection pattern. Martin Fowler, 23 jan. 2004. Disponível em: https://martinfowler.com/articles/injection.html. Acesso em: 02 jun. 2026.


## Autenticação

O projeto consiste em um a API que usa autenticação.

O Identity Provider (IdP) escolhido é o `Auth0` (https://auth0.com/)

Você não precisa de uma conta para ele.

Para emitir o JWT, apenas use a `collection` do `postman` em `/e2e`

Nessa collection há um request para obter o token de acesso.

Nela, já está configurado as credenciais default de acesso.

## Executar Projeto

Para executar o projeto, temos 3 alternativas descritas abaixo.

Siga apenas 1 delas.

### Pré-requisito

Precisa instalar o dotnet 10.x
https://dotnet.microsoft.com/pt-br/download/dotnet/thank-you/sdk-10.0.301-windows-x64-installer

ou

Docker (https://www.docker.com/) ou Podman (https://podman.io/) instalado

### Alternativa A - Containers (docker, podman, ...)

Passo 1 - Com o console apontado para o root do repositório, execute `docker-compose up -d -b`

Se estiver usando o podman, use `podman compose up -d --build`

E pronto!

o banco de dados `postgres` e a `api` estarão disponíveis.

Passo 2 - Use `http://localhost:8080/index.html` para acessar o swagger.

### Alternativa B - Local com dotnet cli

Passo 1 - rode o `postgres` - pode ser uma instancia local ou via container `docker-compose run postgres -d`.

Passo 2 - aponte o console para a pasta `server/TechChallenge.Oficina.API/` e então execute `dotnet run`

Pronto!

Vai subir a API usando https com um certificado autoassinado do dotnet.

Passo 3 - Use `https://localhost:7194/index.html` para acessar o Swagger. 

### Alternativa C - Local com visual studio 2026 ou vs code

Passo 1 - rode o `postgres` - pode ser uma instancia local ou via container `docker-compose run postgres -d`.

Passo 2 - abra o arquivo `.slnx` em `/server`

Passo 3 - no visual studio, rode usando o perfil `https`.

Passo 4 - Use `https://localhost:7194/index.html` para acessar o Swagger. 

## Observações Gerais

- Para fazer requisições, use a  `collection` do `postman` na pasta `/e2e`
- Para o envio de emails, é preciso configurar `ApiKey` no appsettings.json ou `ResendSettings__ApiKey` para container

Você pode logar um github em https://resend.com/, criar sua conta e gerar a apiKey.

- Sem a apiKey, a API funciona normal, só nao envia os emails.
