# Plano de infraestrutura AWS com Terraform

## 1. Objetivo

Criar, por Terraform, a infraestrutura AWS necessária para executar as APIs do projeto no Amazon EKS e disponibilizar os endpoints por HTTP usando o hostname público gerado pela AWS para o Load Balancer.

A aplicação dos manifests das APIs continuará sendo manual, executada posteriormente com `kubectl`. Como exceção controlada, o Terraform deverá instalar o `ingress-nginx` via Helm para criar o controller e seu Service público do tipo `LoadBalancer`. O Terraform não deverá ler, aplicar, importar ou gerenciar os manifests de aplicação da pasta `k8s/`.

O código Terraform definitivo ficará dentro de `infra/`, separado em duas configurações: `infra/foundation/` para a infraestrutura AWS e `infra/addons/` para os charts Helm instalados no EKS. A pasta `infra/example-only/` será usada apenas como referência durante a implementação e poderá ser removida depois.

## 2. Evidências levantadas no repositório

Os manifests em `k8s/` indicam que:

- O namespace das APIs é `oficina`.
- Existem cinco APIs: `monolith-api`, `approval-api`, `createos-api`, `getos-api` e `status-api`.
- Cada API usa `Deployment`, `Service` `ClusterIP`, `ConfigMap`, `Secret` e HPA.
- Os containers escutam na porta `8080`; os Services expõem a porta `80`.
- As imagens já estão publicadas em um Amazon ECR existente na região `us-east-1`, no registry `903936907231.dkr.ecr.us-east-1.amazonaws.com`. Os nodes do EKS farão pull usando sua role IAM, sem Secret Kubernetes de registry.
- A região AWS está definida como `us-east-1` para o bucket de state e para toda a infraestrutura provisionada pelo Terraform.
- O manifesto de Ingress roteia por caminho: `/monolith`, `/approval`, `/createos`, `/getos` e `/status`.
- O Ingress das APIs não restringe mais o campo `host`, permitindo acesso pelo hostname público do Load Balancer gerado pela AWS.
- O manifesto `k8s/infra/ingress.yml` contém somente o Ingress das APIs. O controller deverá ser instalado pelo Terraform via Helm, e esse manifesto poderá ser aplicado manualmente depois que o controller estiver pronto.
- Os manifests em `k8s/db-local/` são exclusivos para ambiente local e não deverão ser aplicados na AWS.
- As APIs esperam uma connection string PostgreSQL e algumas esperam também `ResendSettings__ApiKey`.
- Os HPA dependem de métricas de recursos disponíveis no cluster, portanto o Metrics Server deverá existir no cluster antes da aplicação/validação dos HPA. Como não há manifestos do Metrics Server em `k8s/`, ele será instalado pelo Terraform via Helm, seguindo a mesma exceção controlada do `ingress-nginx`.

## 3. Escopo do Terraform

### 3.1 Recursos incluídos

O código deverá provisionar:

1. Uso de backend remoto do Terraform em um bucket Amazon S3 existente, com versionamento, criptografia, bloqueio de acesso público e permissões previamente configurados no bucket.
2. VPC dedicada para o projeto.
3. Subnets públicas em pelo menos duas Availability Zones para o Load Balancer e os nodes do EKS.
4. Subnets privadas em pelo menos duas Availability Zones para o RDS.
5. Internet Gateway e tabelas de rotas públicas.
6. Sem NAT Gateway: os nodes ficarão em subnets públicas para reduzir custo e simplificar a saída para ECR e demais serviços AWS.
7. Security Groups para EKS/control plane, nodes e RDS, com regras mínimas necessárias. O Load Balancer e suas regras serão gerenciados pelo Kubernetes/AWS.
8. IAM Roles e policies para o control plane e os managed node groups, sem depender de ARNs fixos de laboratório.
9. Cluster Amazon EKS com versão parametrizada e modo de autenticação compatível com `aws eks update-kubeconfig`.
10. Managed node group com tipo de instância, capacidade inicial e limites de escala parametrizados.
11. Componentes padrão necessários do EKS, mantidos pela própria plataforma quando aplicável.
12. Provider Helm configurado no state separado de addons.
13. Instalação do chart oficial `ingress-nginx`, incluindo namespace próprio e Service público do tipo `LoadBalancer`.
14. Instalação do chart oficial do Metrics Server via Helm, sem aplicar manifests da pasta `k8s/`.
15. Amazon RDS for PostgreSQL em subnets privadas, usando DB subnet group, Security Group próprio, credenciais fora do código e parâmetros configuráveis.
16. Outputs para conexão ao cluster, endpoint do RDS e hostname do Load Balancer do ingress-nginx, obtido operacionalmente com `kubectl`.
17. Configuração de acesso IAM ao cluster para o usuário IAM `cluster_admin`, cujo ARN será descoberto pelo Terraform.

O ambiente não armazenará dados importantes. O RDS fará parte do mesmo state da infraestrutura foundation e será destruído junto com a VPC, o EKS e os demais recursos quando `terraform destroy` for executado na configuração foundation. Não será usada proteção `lifecycle.prevent_destroy` nem preservação de snapshot final.

Como o ambiente é de estudos, a configuração inicial deverá priorizar simplicidade e baixo custo: um managed node group pequeno, com uma instância `t3.medium`, `desired_size = 1`, `min_size = 1` e `max_size = 2`, e um RDS PostgreSQL `db.t3.micro`, com armazenamento inicial mínimo. Esses valores deverão ser variáveis para permitir ajuste caso a carga aumente.

### 3.2 Recursos explicitamente excluídos

O Terraform não deverá criar ou gerenciar:

- `Namespace`, `Deployment`, `Service` das APIs, `Ingress` das APIs, `HPA`, `ConfigMap`, `Secret` ou qualquer outro recurso de aplicação da pasta `k8s/`.
- Os recursos específicos do ingress-nginx por manifests YAML. O controller será instalado pelo chart Helm, gerenciado pelo Terraform.
- Secrets Kubernetes para autenticação no ECR. O pull das imagens será feito pela role IAM dos nodes.
- O Postgres definido em `k8s/db-local/`.
- Repositórios, imagens e políticas de lifecycle do Amazon ECR. O registry já existente será apenas consumido pelo EKS.
- Aplicação dos manifests da aplicação com `kubectl`, `kubernetes_manifest`, `kubectl_manifest` ou comandos locais.
- Certificados TLS, HTTPS, Route 53 ou domínio próprio nesta primeira etapa.
- Pipelines de build, push ou deploy das imagens.

O provider Kubernetes poderá ser omitido do Terraform. O provider Helm será usado exclusivamente para instalar e atualizar os charts `ingress-nginx` e Metrics Server; ele não deverá receber manifests das APIs nem gerenciar seus recursos. O hostname do Load Balancer será obtido com `kubectl`, sem adicionar o provider Kubernetes apenas para consulta.

## 4. Arquitetura proposta

```text
Internet
   |
   | HTTP :80
   v
Load Balancer público criado pelo Service do ingress-nginx instalado via Helm
   |
   v
Ingress Controller no EKS
   |
   +--> /monolith  --> monolith-api:80 --> container:8080
   +--> /approval  --> approval-api:80 --> container:8080
   +--> /createos  --> createos-api:80 --> container:8080
   +--> /getos     --> getos-api:80 --> container:8080
   +--> /status    --> status-api:80 --> container:8080

EKS nodes em subnets públicas, sem acesso direto permitido pela Internet
   |
   v
RDS PostgreSQL em subnets privadas
```

### 4.1 Regras de exposição da rede

Os nodes públicos não deverão expor as APIs diretamente. Os Security Groups deverão seguir estas regras mínimas:

- HTTP/HTTPS público somente no Load Balancer criado pelo `ingress-nginx`.
- Nenhuma regra de entrada `0.0.0.0/0` para as portas dos containers (`8080`) ou dos Services.
- O Terraform não deverá fixar ou abrir NodePorts. O Service `LoadBalancer` do chart e a integração Kubernetes/AWS deverão gerenciar os NodePorts e as regras necessárias entre Load Balancer e nodes.
- SSH não será aberto para a Internet; acesso administrativo aos nodes não faz parte do fluxo.
- O RDS ficará sem acesso público e aceitará TCP `5432` somente a partir do Security Group dos nodes do EKS.
- Os nodes poderão ter saída para os serviços AWS necessários, incluindo ECR.

### 4.2 Acesso público inicial

A primeira opção será usar o hostname DNS atribuído pela AWS ao Service `LoadBalancer` do ingress-nginx, por exemplo:

```text
http://<hostname-do-load-balancer>/monolith
http://<hostname-do-load-balancer>/approval
http://<hostname-do-load-balancer>/createos
http://<hostname-do-load-balancer>/getos
http://<hostname-do-load-balancer>/status
```

Esse hostname não é conhecido imediatamente, pois depende da criação do Service `LoadBalancer` pelo chart Helm e da reconciliação do mecanismo de provisionamento configurado no EKS. O Terraform deverá expor o status/hostname quando estiver disponível, usando consulta read-only se necessário; a validação operacional também deverá permitir obtê-lo com `kubectl`.

O campo `host` foi removido do Ingress das APIs. Essa é a opção adotada para a primeira versão: as regras serão atendidas pelo hostname DNS público gerado pela AWS para o Load Balancer, sem Route 53 e sem domínio próprio. O chart `ingress-nginx` usará os valores padrão para o Service `LoadBalancer`, sem annotations ou subnets customizadas nesta etapa.

## 5. Organização planejada dos arquivos

A implementação deverá substituir a organização do exemplo por uma estrutura semelhante a:

```text
infra/
   PLANO-INFRAESTRUTURA-AWS-TERRAFORM.md
   foundation/
      versions.tf
      providers.tf
      variables.tf
      locals.tf
      backend.tf
      data.tf
      vpc.tf
      routes.tf
      security-groups.tf
      iam.tf
      eks.tf
      eks-node-groups.tf
      rds.tf
      outputs.tf
      terraform.tfvars.example
   addons/
      versions.tf
      providers.tf
      backend.tf
      ingress-nginx.tf
      metrics-server.tf
      outputs.tf
      terraform.tfvars.example
   README.md
```

As duas configurações usarão o mesmo bucket S3, mas chaves diferentes: `techchallenge-oficina/foundation.tfstate` e `techchallenge-oficina/addons.tfstate`. Os states não deverão compartilhar recursos. A configuração `addons` receberá o nome do cluster por variável e consultará o endpoint, certificado e token por data sources AWS; não deverá ler o state remoto da foundation.

## 6. Backend remoto do Terraform

Os states deverão ficar no bucket S3 já criado manualmente:

```text
terraform-state-soat16
```

Esse bucket é um recurso externo e não será criado, importado, alterado ou destruído pelo Terraform. Cada configuração Terraform deverá usar uma chave exclusiva para evitar conflito entre os states.

O bucket deverá possuir, ou deverá ser confirmado antes do `terraform init`, o seguinte:

- Versionamento habilitado.
- Criptografia server-side habilitada.
- Bloqueio de acesso público.
- Bucket owner enforced, sem ACL pública.
- Permissões para o usuário ou role utilizada pela AWS CLI executar leitura e escrita do state.
- Locking compatível com a versão do Terraform adotada. Para versões que suportem o locking nativo, usar `use_lockfile = true` no backend S3.

O `backend.tf` de `infra/foundation/` deverá ser configurado conceitualmente assim:

```hcl
terraform {
   backend "s3" {
      bucket       = "terraform-state-soat16"
      key          = "techchallenge-oficina/foundation.tfstate"
      region       = "us-east-1"
      encrypt      = true
      use_lockfile = true
   }
}
```

O `backend.tf` de `infra/addons/` usará os mesmos parâmetros, mas com `key = "techchallenge-oficina/addons.tfstate"`. O bucket não deverá aparecer como `resource "aws_s3_bucket"` e não deverá fazer parte de nenhum `terraform destroy`.

## 7. Rede

A VPC deverá ser criada com DNS support e DNS hostnames habilitados.

Distribuição planejada:

- Pelo menos duas Availability Zones na região escolhida.
- Subnets públicas para Load Balancers e nodes do EKS.
- Subnets privadas isoladas para o RDS, usando um DB subnet group com pelo menos duas AZs.
- Saída direta dos nodes pelas rotas públicas; não haverá NAT Gateway neste ambiente de estudos.

Os CIDRs e a quantidade de AZs deverão ser variáveis. A exposição pública dos nodes será limitada por Security Groups e não deverá criar acesso direto às APIs.

## 8. EKS e IAM

O cluster deverá usar roles IAM criadas pelo próprio Terraform, com trust policies restritas aos serviços AWS corretos. Não deverão ser usados os ARNs fixos `LabRole` ou `voclabs` do exemplo.

O node group deverá usar uma role com, no mínimo, as permissões necessárias para:

- Registrar os nodes no cluster.
- Usar a interface de rede VPC CNI.
- Fazer pull das imagens privadas no ECR usando a policy gerenciada `AmazonEC2ContainerRegistryReadOnly`.

O plano deverá preferir EKS managed node groups e um grupo inicial pequeno, parametrizado por:

- Tipo de instância, com default de estudo `t3.medium`.
- Disco com tamanho mínimo compatível com o EKS e as imagens utilizadas.
- `min_size = 1`, `desired_size = 1` e `max_size = 2` como defaults de estudo.
- Labels e taints, se necessários.

O EKS control plane não deve ser considerado free tier por padrão. O plano deverá alertar que EKS, Load Balancer e RDS podem gerar cobrança mesmo com nodes e banco nas menores classes. Para o RDS, a elegibilidade ao free tier depende da conta, da região, da engine, da classe e do período promocional vigente; isso deverá ser confirmado antes do `apply`. O perfil de estudo deverá usar armazenamento mínimo e RDS Single-AZ, aceitando menor resiliência para reduzir custo.

O acesso administrativo deverá ser concedido via EKS Access Entry ao usuário IAM `cluster_admin`, criado manualmente. O ARN será descoberto pelo Terraform por nome, sem ser gravado diretamente no código:

```hcl
data "aws_iam_user" "cluster_admin" {
   user_name = "cluster_admin"
}
```

Esse usuário será usado somente para administrar o cluster com `kubectl`. O usuário ou profile AWS utilizado para executar o Terraform continuará sendo responsável por criar a infraestrutura e as associações de acesso do EKS. O Terraform também deverá criar a associação com a policy gerenciada `AmazonEKSClusterAdminPolicy`, em escopo de cluster, para o ARN descoberto do usuário `cluster_admin`.

O Terraform deverá produzir o comando equivalente a:

```powershell
aws eks update-kubeconfig --region <regiao> --name <nome-do-cluster>
```

## 9. ECR e imagens existentes

O ECR não faz parte do escopo de criação do Terraform. O registry já existe e as imagens já foram publicadas em:

```text
903936907231.dkr.ecr.us-east-1.amazonaws.com
```

Os Deployments usam os seguintes repositórios existentes:

- `techchallenge-oficina-monolith`
- `techchallenge-oficina-approval`
- `techchallenge-oficina-createos`
- `techchallenge-oficina-getos`
- `techchallenge-oficina-status`

O Terraform deverá apenas:

- Conceder aos nodes do EKS permissão de leitura no ECR existente.
- Manter o registry como variável de configuração, sem criar recursos ECR.
- Preservar os nomes e tags das imagens já referenciados pelos manifests.

Os Deployments não usarão `imagePullSecrets`. A role IAM dos nodes deverá possuir a policy gerenciada `AmazonEC2ContainerRegistryReadOnly`, e os nodes deverão ter conectividade de rede com o ECR. O Terraform não criará Secret Kubernetes para autenticação no registry.

## 10. RDS PostgreSQL

O RDS deverá substituir o Postgres de `k8s/db-local/`.

Configuração planejada:

- Engine PostgreSQL em versão suportada pela região.
- Classe inicial de estudo `db.t3.micro`, escolhida para manter o custo baixo e simplificar a configuração.
- Armazenamento inicial mínimo, preferencialmente 20 GiB, sem provisionar capacidade excedente.
- DB subnet group nas subnets privadas.
- Security Group permitindo TCP `5432` somente a partir do Security Group dos nodes/pods que acessarão o banco.
- Banco, usuário e porta parametrizados.
- Senha do RDS e chave da Resend fornecidas por variáveis Terraform marcadas como `sensitive = true`.
- Valores sensíveis fornecidos por um arquivo `.tfvars` local, variável de ambiente ou outro mecanismo fora do Git; nenhum segredo real deverá ser versionado.
- `sensitive = true` evita exibição acidental em outputs, mas não remove os valores do Terraform state; por isso a criptografia e as permissões restritas do bucket S3 são obrigatórias.
- Backup, retenção e encryption at rest configuráveis; `deletion_protection = false` para permitir a destruição do ambiente de estudos.
- O ambiente é descartável para estudo: não há necessidade de preservar snapshots finais.
- Não será usado `lifecycle.prevent_destroy`; o RDS será destruído junto com a foundation quando `terraform destroy` for executado.
- `skip_final_snapshot = true` será usado porque o ambiente não armazenará dados importantes.
- Multi-AZ inicialmente opcional, devido ao custo, mas previsto no desenho.

O endpoint e a porta do RDS deverão sair em outputs para que a connection string dos Secrets das APIs seja atualizada manualmente antes de aplicar os Deployments. O host `postgres` dos manifests locais não funcionará na AWS. O Terraform não criará nem gerenciará os Secrets Kubernetes das APIs; os valores deverão ser usados manualmente nos manifests ou no procedimento operacional adotado.

## 11. Ordem planejada de execução

### Fase 0: decisões e pré-requisitos

- Confirmar account AWS, perfil da AWS CLI e ambiente (`dev`, `homolog` ou `prod`).
- Confirmar que o bucket S3 `terraform-state-soat16` possui versionamento, criptografia, bloqueio público e permissões de state configurados.
- Confirmar CIDRs que não colidem com a rede de acesso.
- Confirmar que o usuário IAM `cluster_admin` foi criado manualmente e possui uma credencial utilizável pelo `kubectl`.
- Definir tamanho inicial do EKS e do RDS.
- Definir como as variáveis sensíveis serão fornecidas sem serem versionadas.

### Fase 1: backend

- Validar o bucket S3 externo `terraform-state-soat16` e suas permissões.
- Configurar e inicializar o backend S3 de `foundation/` com a chave `techchallenge-oficina/foundation.tfstate`.
- Executar `terraform init` dentro de `foundation/` e validar que o state está no S3.
- Depois da foundation pronta, configurar e inicializar o backend S3 de `addons/` com a chave `techchallenge-oficina/addons.tfstate`.
- Validar que os dois states são independentes e não usam a mesma chave.

### Fase 2: infraestrutura AWS

- Executar `terraform apply` em `foundation/` para criar VPC, subnets, gateways, rotas, Security Groups, roles IAM, EKS, node group, add-ons AWS e RDS.
- Criar a Access Entry e a associação de policy do usuário `cluster_admin` no state da foundation.
- Validar o cluster, os nodes, o endpoint e os outputs da foundation.
- Inicializar `addons/` somente depois que o cluster e os nodes estiverem prontos, informando o nome do cluster por variável.
- Executar `terraform apply` em `addons/` para configurar o provider Helm.
- Instalar o chart `ingress-nginx` via Helm, com namespace próprio e Service `LoadBalancer` público usando os valores padrão.
- Instalar o chart do Metrics Server via Helm e aguardar seu Deployment ficar disponível.

### Fase 3: outputs e acesso ao cluster

- Executar `terraform output`.
- Configurar o kubeconfig com `aws eks update-kubeconfig`.
- Validar nodes, namespaces padrão e add-ons com `kubectl`.
- Confirmar conectividade dos nodes com ECR e serviços AWS necessários.

### Fase 4: aplicação manual dos manifests

Esta fase fica fora do Terraform:

1. Ajustar os Secrets das APIs para o endpoint e credenciais do RDS.
2. Não aplicar `k8s/db-local/`.
3. Aplicar `k8s/base`.
4. Aplicar `k8s/infra/ingress.yml` depois que o controller instalado pelo Helm estiver pronto.
5. Aplicar `k8s/features`.
6. Obter o hostname do Service `LoadBalancer` do ingress-nginx criado pelo Helm.
7. Testar as cinco URLs HTTP.

## 12. Validação esperada

A implementação Terraform deverá ser validada, no mínimo, com:

```powershell
Push-Location foundation
terraform fmt -check
terraform init
terraform validate
terraform plan
terraform apply
Pop-Location

Push-Location addons
terraform fmt -check
terraform init
terraform validate
terraform plan
terraform apply
Pop-Location
```

Depois do provisionamento:

```powershell
aws eks update-kubeconfig --region <regiao> --name <cluster>
kubectl get nodes
kubectl get addons --cluster-name <cluster> --region <regiao>
kubectl get svc -A
kubectl get ingress -n oficina
```

Para destruir todo o ambiente, remover primeiro os recursos Helm enquanto o cluster ainda existe e depois destruir a foundation:

```powershell
Push-Location addons
terraform destroy
Pop-Location

Push-Location foundation
terraform destroy
Pop-Location
```

Após a aplicação manual dos manifests, validar:

- Pods Ready e sem erros de pull de imagem.
- Services apontando para endpoints.
- Metrics Server disponível e retornando métricas.
- HPA com métricas disponíveis.
- Ingress Controller com Load Balancer público.
- Resolução do hostname DNS da AWS.
- HTTP `200` ou resposta esperada nos endpoints de health e nas rotas das cinco APIs.
- Conectividade das APIs com o RDS.

## 13. Custos, segurança e destruição

O plano deverá destacar que EKS, RDS, Load Balancer e nodes geram custo. Para ambiente de estudo, deverá haver variáveis explícitas para reduzir custo, sem transformar regras de segurança em defaults perigosos.

Antes de `terraform destroy`, deverá ser verificado:

- Se o bucket de state externo `terraform-state-soat16` permanece fora do escopo de destruição.
- Se a operação de destruição será executada primeiro em `addons/` e depois em `foundation/`.
- Se o ambiente continua sem dados importantes antes de destruir o RDS.
- Se `skip_final_snapshot = true` está configurado para o RDS descartável.
- Se ECR contém imagens que precisam ser preservadas.

## 14. Critério de pronto do plano

O plano será considerado aprovado quando houver concordância sobre:

- A topologia de rede.
- A versão e o tamanho do EKS.
- A configuração e o custo aceitável do RDS e da rede simplificada.
- O uso do bucket externo `terraform-state-soat16` com chaves separadas para `foundation` e `addons`.
- O acesso administrativo do usuário IAM `cluster_admin` ao EKS.
- O formato das URLs HTTP.
- A responsabilidade manual pela aplicação dos manifests Kubernetes.

Somente depois dessa aprovação deverão ser criados os arquivos `.tf` dentro de `infra/`.
