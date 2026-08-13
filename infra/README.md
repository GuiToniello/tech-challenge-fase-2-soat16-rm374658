# Terraform AWS

Tutorial resumido para criar a infraestrutura do trabalho. Existem duas configurações Terraform independentes:

- `foundation/`: VPC, subnets, IAM, EKS, node group e RDS.
- `addons/`: `ingress-nginx` e Metrics Server via Helm.

O state usa o bucket S3 externo `terraform-state-soat16`, com estas chaves:

- `techchallenge-oficina/foundation.tfstate`
- `techchallenge-oficina/addons.tfstate`

Detalhes da arquitetura estão em [ESTRUTURA.md](ESTRUTURA.md).

## 1. Pré-requisitos

Crie/configure manualmente na AWS:

- Bucket S3 `terraform-state-soat16` em `us-east-1`, com versionamento, criptografia, bloqueio público e acesso de leitura/escrita.
- Usuário IAM `terraform`, usado para executar Terraform, Helm e `kubectl`.
- Usuário IAM `cluster_admin`, mantido como acesso alternativo ao EKS.
- AWS CLI configurada com o profile que será usado pelo Terraform.

O Terraform não cria esses usuários nem o bucket.

## 2. Crie os arquivos de variáveis

É necessário criar um `terraform.tfvars` em cada pasta Terraform. A partir da pasta `infra/`, execute:

```powershell
Copy-Item foundation/terraform.tfvars.example foundation/terraform.tfvars
Copy-Item addons/terraform.tfvars.example addons/terraform.tfvars
```

Edite `foundation/terraform.tfvars` e informe a senha real do RDS em:

```hcl
rds_password = "SUA_SENHA"
```

Não versione esses arquivos. Eles já estão ignorados pelo `.gitignore`.

## 3. Crie a foundation

Execute os comandos dentro de `infra/foundation`:

```powershell
Push-Location foundation
terraform init
terraform fmt -check -recursive
terraform validate
terraform plan -var-file="terraform.tfvars"
terraform apply -var-file="terraform.tfvars"
Pop-Location
```

O `apply` é manual. Revise o `plan` antes de confirmar.

## 4. Instale os addons

Depois que o node do EKS estiver `Ready`, execute:

```powershell
Push-Location addons
terraform init
terraform fmt -check -recursive
terraform validate
terraform plan -var-file="terraform.tfvars"
terraform apply -var-file="terraform.tfvars"
Pop-Location
```

Isso instala o `ingress-nginx` e o Metrics Server. O mesmo usuário/profile `terraform` é usado nos dois estados.

## 5. Configure o kubectl

```powershell
aws eks update-kubeconfig --region us-east-1 --name techchallenge-oficina-eks
kubectl get nodes
kubectl get pods -A
kubectl get svc ingress-nginx-controller -n ingress-nginx
```

## 6. Aplique as APIs

O arquivo `k8s/.env` contém a connection string do RDS e a chave do Resend. Ele é ignorado pelo Git. O arquivo `k8s/kustomization.yaml` usa esse `.env` para gerar o Secret `oficina-api-secrets`.

A partir da raiz do projeto:

```powershell
kubectl apply -k k8s
```

Não aplique `k8s/db-local/` na AWS.

Valide:

```powershell
kubectl get pods,svc,hpa,ingress -n oficina
kubectl top pods -n oficina
```

## 7. Destrua o ambiente

Para evitar custos, destrua primeiro os addons e depois a foundation:

```powershell
Push-Location addons
terraform destroy -var-file="terraform.tfvars"
Pop-Location

Push-Location foundation
terraform destroy -var-file="terraform.tfvars"
Pop-Location
```

O bucket S3 do state e o ECR existente ficam fora do `destroy`.