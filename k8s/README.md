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
- [k8s/base/namespace.yml](k8s/base/namespace.yml)
- [k8s/infra/ingress.yml](k8s/infra/ingress.yml)
- [k8s/features/monolith-api](k8s/features/monolith-api)
- [k8s/features/approval-api](k8s/features/approval-api)
- [k8s/features/createos-api](k8s/features/createos-api)
- [k8s/features/getos-api](k8s/features/getos-api)
- [k8s/features/status-api](k8s/features/status-api)
- [k8s/db-local](k8s/db-local)

Cada pasta de API contem:
- configmap.yml
- secret.yml
- deployment.yml
- service.yml
- hpa.yml

A pasta db-local contem os manifestos do Postgres para uso exclusivamente local:
- [k8s/db-local/configmap.yml](k8s/db-local/configmap.yml)
- [k8s/db-local/secret.yml](k8s/db-local/secret.yml)
- [k8s/db-local/pvc.yml](k8s/db-local/pvc.yml)
- [k8s/db-local/deployment.yml](k8s/db-local/deployment.yml)
- [k8s/db-local/service.yml](k8s/db-local/service.yml)

Na nuvem, os manifestos de [k8s/db-local](k8s/db-local) nao devem ser aplicados. O banco de dados deve ser provisionado pelo Amazon RDS.

Observacao: Postgres nao possui HPA por decisao de arquitetura solicitada.

## 3. Ordem recomendada de aplicacao
1. Aplicar base (namespace): `kubectl apply -R -f k8s/base`
2. Aplicar o Ingress das APIs; os recursos do controller são instalados pelo Terraform via Helm.
3. Em ambiente local, aplicar o Postgres: `kubectl apply -R -f k8s/db-local`
4. Aplicar features (APIs): `kubectl apply -R -f k8s/features`
5. Validar pods, services, hpas, pvc e ingress.
6. Validar o estado do ingress controller.

Comandos para aplicar em ordem na AWS:
- kubectl apply -R -f k8s/base
- kubectl apply -f k8s/infra/ingress.yml
- kubectl apply -R -f k8s/features

O controller e o Metrics Server sao instalados pelo Terraform via Helm. Os manifestos de `k8s/db-local` nao devem ser aplicados na AWS.

Comando pratico para verificacao:
- kubectl get pods,svc,hpa,pvc,ingress -n oficina
- kubectl get ingressclass
- kubectl get pods,svc -n ingress-nginx

Opcional (ambiente local): para acessar via localhost, encaminhar portas do ingress controller:
- kubectl port-forward -n ingress-nginx service/ingress-nginx-controller 80:80 443:443

## 4. Recursos criados por API
Para cada API foram criados os seguintes recursos:
1. ConfigMap com configuracoes nao sensiveis.
2. Secret com configuracoes sensiveis.
3. Deployment com 1 replica inicial, probes e limites de recursos.
4. Service do tipo ClusterIP.
5. HPA (autoscaling/v2).
6. Pull das imagens privadas no ECR usando a role IAM dos nodes do EKS.

### 4.1 APIs contempladas
- monolith-api
- approval-api
- createos-api
- getos-api
- status-api

### 4.2 Imagens utilizadas
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

### 4.3 Autenticacao no ECR privado
No EKS, a role IAM dos nodes devera possuir a policy gerenciada `AmazonEC2ContainerRegistryReadOnly`, permitindo que o kubelet faca pull das imagens privadas do ECR sem Secret Kubernetes.

A mesma regiao AWS facilita o acesso ao registry, mas nao substitui a permissao IAM nem a conectividade de rede dos nodes ao ECR.

## 5. Recursos do Postgres local
Para execucao local, o Postgres foi modelado com:
1. Deployment usando imagem postgres:17-alpine.
2. Service ClusterIP chamado postgres na porta 5432.
3. ConfigMap com POSTGRES_DB e POSTGRES_USER.
4. Secret com POSTGRES_PASSWORD.
5. PVC para persistencia de dados em /var/lib/postgresql/data.
6. Probes de readiness e liveness com pg_isready.

Esses manifestos existem apenas para o Kubernetes local. Em ambientes de nuvem, o banco deve ser executado no Amazon RDS, sem aplicar os recursos de [k8s/db-local](k8s/db-local).

## 6. Comunicacao entre APIs e banco
No ambiente local, a comunicacao entre APIs e Postgres usa o DNS interno do cluster por meio do Service ClusterIP.

Host do banco nas connection strings locais:
- postgres

Connection string aplicada nos Secrets das APIs:
- Host=postgres;Port=5432;Database=oficina;Username=sa;Password=P@ssw0rd123

Arquivos atualizados:
- [k8s/features/monolith-api/secret.yml](k8s/features/monolith-api/secret.yml)
- [k8s/features/approval-api/secret.yml](k8s/features/approval-api/secret.yml)
- [k8s/features/createos-api/secret.yml](k8s/features/createos-api/secret.yml)
- [k8s/features/getos-api/secret.yml](k8s/features/getos-api/secret.yml)
- [k8s/features/status-api/secret.yml](k8s/features/status-api/secret.yml)

Na nuvem, as APIs devem usar a connection string e as credenciais fornecidas pelo Amazon RDS. Os Secrets das APIs precisam ser ajustados para o endpoint do RDS, em vez do host `postgres`.

## 7. Regras de HPA aplicadas
Foi seguido o requisito informado:
1. CPU alvo: 30% de utilizacao.
2. Memoria alvo: 80% de utilizacao.
3. Minimo: 1 replica.
4. Maximo: 10 replicas.

Aplicado em:
- [k8s/features/monolith-api/hpa.yml](k8s/features/monolith-api/hpa.yml)
- [k8s/features/approval-api/hpa.yml](k8s/features/approval-api/hpa.yml)
- [k8s/features/createos-api/hpa.yml](k8s/features/createos-api/hpa.yml)
- [k8s/features/getos-api/hpa.yml](k8s/features/getos-api/hpa.yml)
- [k8s/features/status-api/hpa.yml](k8s/features/status-api/hpa.yml)

## 8. Namespace
Todos os recursos foram definidos no namespace oficina:
- [k8s/base/namespace.yml](k8s/base/namespace.yml)

## 9. Ingress e acesso externo
Foi criado um manifesto de Ingress para exposicao HTTP das APIs fora do cluster:
- [k8s/infra/ingress.yml](k8s/infra/ingress.yml)

Configuracao aplicada:
1. Sem campo `host`, permitindo o hostname DNS publico gerado pela AWS.
2. Roteamento por path para cada API.
3. Reescrita de URL para remover o prefixo antes de encaminhar ao backend.
4. Backends apontando para Services ClusterIP na porta 80.
5. O `ingressClassName: nginx` referencia o controller instalado pelo Terraform via Helm.

Rotas disponiveis:
1. http://<hostname-do-load-balancer>/monolith
2. http://<hostname-do-load-balancer>/approval
3. http://<hostname-do-load-balancer>/createos
4. http://<hostname-do-load-balancer>/getos
5. http://<hostname-do-load-balancer>/status

Observacao importante:
1. O recurso Ingress das APIs usa `ingressClassName: nginx`.
2. O Ingress Controller nginx e instalado e gerenciado pelo Terraform via Helm.

Checklist de confirmacao do controller:
1. Validar classe: `kubectl get ingressclass`.
2. Validar pods do controller (namespace padrao do ingress-nginx): `kubectl get pods -n ingress-nginx`.
3. Validar service de entrada: `kubectl get svc -n ingress-nginx`.

O hostname do Load Balancer pode ser consultado com `kubectl get svc -n ingress-nginx`.

## 10. Validacao executada
Foi executada validacao estrutural e sintatica dos manifests com dry-run de cliente:
- kubectl apply --dry-run=client -R -f k8s/base
- kubectl apply --dry-run=client -R -f k8s/infra
- kubectl apply --dry-run=client -R -f k8s/db-local
- kubectl apply --dry-run=client -R -f k8s/features

Resultado:
- Todos os recursos foram aceitos no dry-run sem erros.

## 11. Finalizacao
Antes de promover para ambientes compartilhados (qa/homolog/prod), recomenda-se:
1. Atualizar as tags das imagens no ECR conforme a versao liberada.
2. Ajustar valores de secrets para credenciais reais e chave de email (Resend) quando aplicavel.
3. Revisar requests/limits de CPU e memoria conforme carga real.
4. Garantir instalacao/configuracao do Ingress Controller no cluster (ex.: NGINX Ingress).
5. Configurar as APIs para acessar o Amazon RDS e nao aplicar os manifestos de [k8s/db-local](k8s/db-local) em ambientes de nuvem.
6. Se for necessario expor portas distintas por API (ex.: localhost:7194), usar estrategia alternativa (NodePort/LoadBalancer ou configuracao TCP do controller), pois Ingress HTTP padrao expoe em 80/443.
