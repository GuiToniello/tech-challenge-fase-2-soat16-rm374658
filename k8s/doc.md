# Kubernetes - Estrutura e Implementacao

## 1. Objetivo
Este documento descreve a estrutura criada em [k8s](k8s) para deploy da aplicacao no Kubernetes, com separacao por feature (API) e manifests separados por tipo de recurso (kind).

A implementacao foi baseada em:
- [docker-compose.yml](docker-compose.yml)
- [.env](.env)
- [.env.example](.env.example)
- Dockerfiles das APIs em [server/1 - Frameworks & Drivers](server/1%20-%20Frameworks%20&%20Drivers)

## 2. Estrutura adotada
Foi adotado o padrao folder-by-feature dentro de [k8s](k8s), com uma pasta por servico.

Estrutura final:
- [k8s/namespace.yml](k8s/namespace.yml)
- [k8s/ingress.yml](k8s/ingress.yml)
- [k8s/ecr-regcred.secret.yml](k8s/ecr-regcred.secret.yml)
- [k8s/monolith-api](k8s/monolith-api)
- [k8s/approval-api](k8s/approval-api)
- [k8s/createos-api](k8s/createos-api)
- [k8s/getos-api](k8s/getos-api)
- [k8s/status-api](k8s/status-api)
- [k8s/postgres](k8s/postgres)

Cada pasta de API contem:
- configmap.yml
- secret.yml
- deployment.yml
- service.yml
- hpa.yml

A pasta postgres contem:
- [k8s/postgres/configmap.yml](k8s/postgres/configmap.yml)
- [k8s/postgres/secret.yml](k8s/postgres/secret.yml)
- [k8s/postgres/pvc.yml](k8s/postgres/pvc.yml)
- [k8s/postgres/deployment.yml](k8s/postgres/deployment.yml)
- [k8s/postgres/service.yml](k8s/postgres/service.yml)

Observacao: Postgres nao possui HPA por decisao de arquitetura solicitada.

## 3. Recursos criados por API
Para cada API foram criados os seguintes recursos:
1. ConfigMap com configuracoes nao sensiveis.
2. Secret com configuracoes sensiveis.
3. Deployment com 1 replica inicial, probes e limites de recursos.
4. Service do tipo ClusterIP.
5. HPA (autoscaling/v2).
6. Referencia de imagePullSecrets para autenticacao no ECR privado.

### 3.1 APIs contempladas
- monolith-api
- approval-api
- createos-api
- getos-api
- status-api

### 3.2 Imagens utilizadas
Com base no docker-compose, os Deployments usam os seguintes repositorios de imagem:
- techchallenge-oficina-monolith
- techchallenge-oficina-approval
- techchallenge-oficina-createos
- techchallenge-oficina-getos
- techchallenge-oficina-status

As imagens das APIs estao fixadas no ECR:
- 903936907231.dkr.ecr.us-east-1.amazonaws.com/techchallenge-oficina-monolith:v1.0.0
- 903936907231.dkr.ecr.us-east-1.amazonaws.com/techchallenge-oficina-approval:v1.0.0
- 903936907231.dkr.ecr.us-east-1.amazonaws.com/techchallenge-oficina-createos:v1.0.0
- 903936907231.dkr.ecr.us-east-1.amazonaws.com/techchallenge-oficina-getos:v1.0.0
- 903936907231.dkr.ecr.us-east-1.amazonaws.com/techchallenge-oficina-status:v1.0.0

### 3.3 Autenticacao no ECR privado
Como as imagens estao em registry privado (ECR), os Deployments das APIs foram configurados com:
- imagePullSecrets:
	- name: ecr-regcred

Arquivos atualizados:
- [k8s/monolith-api/deployment.yml](k8s/monolith-api/deployment.yml)
- [k8s/approval-api/deployment.yml](k8s/approval-api/deployment.yml)
- [k8s/createos-api/deployment.yml](k8s/createos-api/deployment.yml)
- [k8s/getos-api/deployment.yml](k8s/getos-api/deployment.yml)
- [k8s/status-api/deployment.yml](k8s/status-api/deployment.yml)

Criacao inicial do secret no namespace oficina:
- aws ecr get-login-password --region us-east-1 | kubectl create secret docker-registry ecr-regcred --docker-server=903936907231.dkr.ecr.us-east-1.amazonaws.com --docker-username=AWS --docker-password-stdin -n oficina

Alternativa por manifesto YAML (arquivo versionado no root de k8s):
- [k8s/ecr-regcred.secret.yml](k8s/ecr-regcred.secret.yml)

Como preencher o arquivo [k8s/ecr-regcred.secret.yml](k8s/ecr-regcred.secret.yml):
1. Campo password: usar o valor puro retornado por `aws ecr get-login-password --region us-east-1`.
2. Campo auth: usar Base64 de `AWS:TOKEN`.

Exemplo (PowerShell) para gerar os dois valores:
- $token = aws ecr get-login-password --region us-east-1
- $auth = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("AWS:$token"))

Aplicacao via arquivo YAML:
- kubectl apply -f k8s/ecr-regcred.secret.yml

Validacao do secret:
- kubectl get secret ecr-regcred -n oficina

Renovacao do token (idempotente, recomendada periodicamente):
- aws ecr get-login-password --region us-east-1 | kubectl create secret docker-registry ecr-regcred --docker-server=903936907231.dkr.ecr.us-east-1.amazonaws.com --docker-username=AWS --docker-password-stdin -n oficina --dry-run=client -o yaml | kubectl apply -f -

Observacao importante:
1. O token do ECR expira periodicamente (tipicamente em 12 horas).
2. Em ambiente nao-EKS, e recomendado automatizar a renovacao.
3. Nao commitar token real no repositorio (manter placeholders no YAML).

## 4. Recursos criados para Postgres
Com base no docker-compose, o Postgres foi modelado com:
1. Deployment usando imagem postgres:17-alpine.
2. Service ClusterIP chamado postgres na porta 5432.
3. ConfigMap com POSTGRES_DB e POSTGRES_USER.
4. Secret com POSTGRES_PASSWORD.
5. PVC para persistencia de dados em /var/lib/postgresql/data.
6. Probes de readiness e liveness com pg_isready.

## 5. Comunicacao interna entre APIs e banco
A comunicacao entre APIs e Postgres foi configurada por DNS interno do cluster via Service ClusterIP.

Host do banco nas connection strings:
- postgres

Connection string aplicada nos Secrets das APIs:
- Host=postgres;Port=5432;Database=oficina;Username=sa;Password=P@ssw0rd123

Arquivos atualizados:
- [k8s/monolith-api/secret.yml](k8s/monolith-api/secret.yml)
- [k8s/approval-api/secret.yml](k8s/approval-api/secret.yml)
- [k8s/createos-api/secret.yml](k8s/createos-api/secret.yml)
- [k8s/getos-api/secret.yml](k8s/getos-api/secret.yml)
- [k8s/status-api/secret.yml](k8s/status-api/secret.yml)

## 6. Regras de HPA aplicadas
Foi seguido o requisito informado:
1. CPU alvo: 30% de utilizacao.
2. Memoria alvo: 80% de utilizacao.
3. Minimo: 1 replica.
4. Maximo: 10 replicas.

Aplicado em:
- [k8s/monolith-api/hpa.yml](k8s/monolith-api/hpa.yml)
- [k8s/approval-api/hpa.yml](k8s/approval-api/hpa.yml)
- [k8s/createos-api/hpa.yml](k8s/createos-api/hpa.yml)
- [k8s/getos-api/hpa.yml](k8s/getos-api/hpa.yml)
- [k8s/status-api/hpa.yml](k8s/status-api/hpa.yml)

## 7. Namespace
Todos os recursos foram definidos no namespace oficina:
- [k8s/namespace.yml](k8s/namespace.yml)

## 8. Ingress e acesso externo
Foi criado um manifesto de Ingress para exposicao HTTP das APIs fora do cluster:
- [k8s/ingress.yml](k8s/ingress.yml)

Configuracao aplicada:
1. Host: localhost.
2. Roteamento por path para cada API.
3. Reescrita de URL para remover o prefixo antes de encaminhar ao backend.
4. Backends apontando para Services ClusterIP na porta 80.
5. IngressClass `nginx` declarada no proprio [k8s/ingress.yml](k8s/ingress.yml).

Rotas disponiveis:
1. http://localhost/monolith
2. http://localhost/approval
3. http://localhost/createos
4. http://localhost/getos
5. http://localhost/status

Observacao importante:
1. O manifesto [k8s/ingress.yml](k8s/ingress.yml) ja cria a IngressClass `nginx` e vincula o Ingress com `ingressClassName: nginx`.
2. A IngressClass so define o roteamento logico; ainda e necessario ter o Ingress Controller nginx instalado e ativo no cluster.

Checklist de confirmacao do controller:
1. Validar classe: `kubectl get ingressclass`.
2. Validar pods do controller (namespace padrao do ingress-nginx): `kubectl get pods -n ingress-nginx`.
3. Validar service de entrada: `kubectl get svc -n ingress-nginx`.

Se o controller ainda nao existir, instalar o ingress-nginx antes de usar as rotas externas.

## 9. Instalacao do Ingress Controller (NGINX)
Comandos para instalar o ingress-nginx no cluster:

- kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/cloud/deploy.yaml

Comandos para verificar se a instalacao ficou operacional:

- kubectl get ingressclass
- kubectl get pods -n ingress-nginx
- kubectl get svc -n ingress-nginx

Opcional (ambiente local): para acessar via localhost, encaminhar portas do service do controller:

- kubectl port-forward -n ingress-nginx service/ingress-nginx-controller 80:80 443:443

## 10. Validacao executada
Foi executada validacao estrutural e sintatica dos manifests com dry-run de cliente:
- kubectl apply --dry-run=client -R -f k8s

Resultado:
- Todos os recursos foram aceitos no dry-run sem erros.

## 11. Ordem recomendada de aplicacao
1. Aplicar namespace.
2. Criar/atualizar o secret ecr-regcred no namespace oficina.
3. Aplicar manifests do Postgres.
4. Aplicar manifests das APIs.
5. Aplicar manifesto de Ingress.
6. Validar pods, services, hpas, pvc e ingress.
7. Validar o estado do ingress controller.

Comando pratico para aplicar tudo de uma vez:
- kubectl apply -R -f k8s

Comando pratico para verificacao:
- kubectl get pods,svc,hpa,pvc,ingress -n oficina
- kubectl get ingressclass
- kubectl get pods,svc -n ingress-nginx

## 12. Ajustes pendentes por ambiente
Antes de promover para ambientes compartilhados (qa/homolog/prod), recomenda-se:
1. Atualizar as tags das imagens no ECR conforme a versao liberada.
2. Ajustar valores de secrets para credenciais reais e chave de email (Resend) quando aplicavel.
3. Revisar requests/limits de CPU e memoria conforme carga real.
4. Garantir instalacao/configuracao do Ingress Controller no cluster (ex.: NGINX Ingress).
5. Automatizar a renovacao do secret de pull do ECR (ecr-regcred).
6. Se for necessario expor portas distintas por API (ex.: localhost:7194), usar estrategia alternativa (NodePort/LoadBalancer ou configuracao TCP do controller), pois Ingress HTTP padrao expoe em 80/443.
