# ADR-011: Envio de E-mail de Orçamento com Resend

**Status**: Aceita

**Contexto**:
Necessidade de notificar o cliente por e-mail quando um orçamento de ordem de serviço estiver disponível, mantendo separação de camadas e baixo acoplamento com provedor externo.

Atualmente o fluxo exige:
- Uso de um contrato de domínio para envio de e-mail (`IOrcamentoEmailSender`), sem dependência do domínio para SDKs externos.
- Integração com provedor de e-mail transacional.
- Validação antecipada de configurações críticas (`ApiKey` e remetente).
- Testabilidade da integração sem acoplar testes ao cliente real do provedor.

**Decisão**:
- O contrato de envio de orçamento por e-mail fica no **Domain** (`IOrcamentoEmailSender`).
- O caso de uso na **Application** (`OrdemServicoService.EnviarOrcamentoPorEmailAsync`) orquestra regras de negócio e dispara o contrato de domínio:
  - exige orçamento gerado;
  - exige cliente existente;
  - exige e-mail do cliente preenchido.
- A implementação concreta fica na **Infra.Email** (`OrcamentoEmailSender`) e monta assunto/corpo HTML com dados da ordem de serviço e do orçamento.
- O provedor escolhido é o **Resend**, encapsulado por um adapter (`ResendClientAdapter`) por meio da interface interna `IResendClient`, isolando o SDK externo do restante da aplicação.
- O registro de DI centraliza configuração e validações em `AddInfraEmail(...)`:
  - falha rápido se `ResendSettings.ApiKey` ou `ResendSettings.FromEmail` não forem informados;
  - registra `ResendSettings` e serviços de envio.
- A API carrega `ResendSettings` via configuração (`appsettings`/environment variables) e inicializa o módulo de e-mail no startup.

**Consequências**:
- ✅ Mantém arquitetura em camadas: domínio define contrato, infraestrutura implementa.
- ✅ Facilita troca futura de provedor (SendGrid, SES etc.) sem alterar regras de negócio da Application/Domain.
- ✅ Melhora testabilidade com mocks de `IOrcamentoEmailSender` e `IResendClient`.
- ✅ Evita subir a aplicação com configuração inválida (fail fast no startup).
- ⚠ O corpo do e-mail está atualmente montado em string HTML na infraestrutura, exigindo cuidado de manutenção/layout.
- ⚠ Falhas de provedor externo podem impactar o envio síncrono do caso de uso (sem fila/retry nesta etapa).

**Alternativas Rejeitadas**:
- Chamar o SDK do Resend diretamente na Application:
  - rejeitada por acoplamento indevido da camada de aplicação com tecnologia externa.
- Colocar lógica de envio no Domain:
  - rejeitada por violar responsabilidade da camada de domínio (integração externa).
- Envio assíncrono via fila já nesta fase:
  - rejeitada por aumento de complexidade operacional para o escopo atual; pode ser evolução futura.
