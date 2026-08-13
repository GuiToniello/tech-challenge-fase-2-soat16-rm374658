# Terraform AWS

Este diretório contém duas configurações Terraform independentes:

- `foundation/`: VPC, subnets, IAM, EKS, node group, RDS e acesso do usuário que executa o Terraform.
- `addons/`: instalação Helm do ingress-nginx e Metrics Server.

O bucket `terraform-state-soat16` é externo e usa duas chaves:

- `techchallenge-oficina/foundation.tfstate`
- `techchallenge-oficina/addons.tfstate`

## Pré-requisitos manuais

Antes de executar o Terraform, crie/configure manualmente na AWS:

- Bucket S3 `terraform-state-soat16` em `us-east-1`, com versionamento, criptografia, bloqueio público e acesso de leitura/escrita.
- Usuário IAM `terraform`, configurado no profile usado para executar o Terraform.
- Usuário IAM `cluster_admin`, consultado pela foundation e mantido como acesso alternativo ao EKS.

O Terraform não cria esses usuários nem o bucket. Não versione access keys, secrets ou `terraform.tfvars`.

Em `ESTRUTURA.md` você pode conferir mais detalhes da arquitetura planejada.

## Ordem de execução

O `apply` não faz parte da automação deste agente. Execute manualmente:

```powershell
Push-Location foundation
terraform init
terraform validate
terraform plan -var-file="terraform.tfvars"
# terraform apply
Pop-Location

Push-Location addons
terraform init
terraform validate
terraform plan -var-file="terraform.tfvars"
# terraform apply
Pop-Location
```

O usuário/profile que executa o Terraform receberá também uma EKS Access Entry com `AmazonEKSClusterAdminPolicy`. Assim, o mesmo profile `terraform` poderá executar a foundation, instalar os addons e usar o `kubectl`, sem alternância de credenciais. O usuário `cluster_admin` permanece como acesso administrativo alternativo.

Depois que os addons estiverem prontos:

```powershell
# conectar no cluster
aws eks update-kubeconfig --region us-east-1 --name techchallenge-oficina-eks

# verificar nodes e ingress
kubectl get nodes
kubectl get svc ingress-nginx-controller -n ingress-nginx
```

Para destruir o ambiente, destrua primeiro `addons` e depois `foundation`, sempre executando os comandos manualmente.