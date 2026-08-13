# Infraestrutura AWS com Terraform

## Objetivo

Provisionar a infraestrutura necessária para executar as cinco APIs no Amazon EKS e expô-las por HTTP usando o hostname DNS público gerado pela AWS para o Load Balancer do `ingress-nginx`.

Os manifests das APIs continuam sendo aplicados manualmente com `kubectl`. O Terraform gerencia apenas a infraestrutura AWS e, no state separado de addons, instala `ingress-nginx` e Metrics Server via Helm.

## Decisões consolidadas

- Região: `us-east-1`.
- Bucket de state externo: `terraform-state-soat16`.
- States:
  - `techchallenge-oficina/foundation.tfstate`.
  - `techchallenge-oficina/addons.tfstate`.
- ECR existente: `903936907231.dkr.ecr.us-east-1.amazonaws.com`.
- Sem Route 53, domínio próprio, certificado ou HTTPS nesta etapa.
- O Ingress das APIs não possui campo `host`.
- O ECR é acessado pela role IAM dos nodes com `AmazonEC2ContainerRegistryReadOnly`; não existe Secret Kubernetes para pull.
- O ambiente é acadêmico e descartável; RDS e demais recursos podem ser destruídos ao final.

## Pré-requisitos manuais

Antes de executar `terraform init`, devem existir na AWS:

1. Bucket S3 `terraform-state-soat16`, em `us-east-1`, com versionamento, criptografia, bloqueio público e permissões de leitura/escrita para o profile usado pelo Terraform.
2. Usuário IAM `terraform`, configurado na AWS CLI e com permissões para criar a foundation, consultar o usuário `cluster_admin` e criar Access Entries no EKS.
3. Usuário IAM `cluster_admin`, consultado pela foundation e mantido como acesso alternativo ao cluster.

O Terraform não cria esses usuários nem o bucket. Access keys, secrets e `terraform.tfvars` real nunca devem ser versionados.

## Arquitetura

```text
Internet
   |
   | HTTP :80
   v
Load Balancer público do Service ingress-nginx
   |
   v
Ingress Controller no EKS
   |
   +--> /monolith  --> monolith-api:80 --> container:8080
   +--> /approval  --> approval-api:80 --> container:8080
   +--> /createos  --> createos-api:80 --> container:8080
   +--> /getos     --> getos-api:80 --> container:8080
   +--> /status    --> status-api:80 --> container:8080

EKS nodes: subnets públicas
RDS PostgreSQL: subnets privadas, sem acesso público
```

Não haverá NAT Gateway. Os nodes usam saída pelas subnets públicas para acessar ECR e demais serviços AWS.

Os Security Groups devem garantir:

- Nenhuma entrada pública nas portas `8080` ou dos Services.
- Nenhum SSH aberto para a Internet.
- Nenhum NodePort configurado manualmente pelo Terraform.
- O Service `LoadBalancer` e a integração Kubernetes/AWS gerenciam NodePorts e regras do Load Balancer.
- RDS acessível somente na porta `5432` a partir do Security Group dos nodes.

## Foundation

Localização: `infra/foundation/`

Responsabilidades:

- VPC e DNS.
- Duas subnets públicas para Load Balancer e nodes.
- Duas subnets privadas para o DB subnet group do RDS.
- Internet Gateway e rotas públicas.
- Security Groups do cluster, nodes e RDS.
- IAM roles do EKS e dos nodes.
- Policy `AmazonEC2ContainerRegistryReadOnly` na role dos nodes.
- EKS `techchallenge-oficina-eks`.
- Managed node group:
  - Instância `t3.small`, elegível ao Free Tier nesta conta.
  - `min_size = 1`.
  - `desired_size = 1`.
  - `max_size = 1`.
  - Disco EBS de `30 GiB` no launch template.
- RDS PostgreSQL:
  - Classe `db.t3.micro`.
  - `20 GiB`.
  - Single-AZ.
  - Sem acesso público.
  - `deletion_protection = false`.
  - `skip_final_snapshot = true`.
- Access Entry e `AmazonEKSClusterAdminPolicy` para o usuário que executa o Terraform.
- Access Entry opcional para `cluster_admin`.

O usuário `terraform` é o acesso principal e executa foundation, addons e `kubectl`, sem troca obrigatória de credenciais.

## Addons

Localização: `infra/addons/`

Responsabilidades:

- Usar o mesmo profile AWS do usuário `terraform`.
- Consultar o cluster pelo nome, sem ler o state remoto da foundation.
- Instalar `ingress-nginx` via Helm com os valores padrão.
- Instalar Metrics Server via Helm.

O hostname do Load Balancer será consultado com:

```powershell
kubectl get svc ingress-nginx-controller -n ingress-nginx
kubectl get svc ingress-nginx-controller -n ingress-nginx -o jsonpath="{.status.loadBalancer.ingress[0].hostname}"
```

## Aplicação manual dos manifests

Depois que foundation e addons estiverem prontos:

```powershell
kubectl apply -f ..\..\k8s\base\namespace.yml
kubectl apply -f ..\..\k8s\infra\ingress.yml
kubectl apply -R -f ..\..\k8s\features
```

Não aplicar `k8s/db-local/` na AWS. Os Secrets das APIs devem ser atualizados manualmente com o endpoint e as credenciais do RDS antes de aplicar os Deployments.

O arquivo `k8s/infra/ingress.yml` contém somente o recurso `Ingress` das APIs.

## Validação

Em `infra/foundation/`:

```powershell
terraform init
terraform fmt -check -recursive
terraform validate
terraform plan -var-file="terraform.tfvars"
```

Em `infra/addons/`:

```powershell
terraform init
terraform fmt -check -recursive
terraform validate
terraform plan -var-file="terraform.tfvars"
```

O `terraform apply` é manual e deve ser executado primeiro na foundation e depois nos addons.

## Destruição e custos

EKS, RDS, nodes e Load Balancer geram custos enquanto existem. Para destruir todo o ambiente:

```powershell
Push-Location infra\addons
terraform destroy -var-file="terraform.tfvars"
Pop-Location

Push-Location infra\foundation
terraform destroy -var-file="terraform.tfvars"
Pop-Location
```

A ordem remove primeiro os charts Helm e depois o cluster, RDS e rede. O bucket S3 do state e o ECR existente permanecem fora do `destroy`.
