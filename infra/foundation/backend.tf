terraform {
  backend "s3" {
    bucket       = "terraform-state-soat16"
    key          = "techchallenge-oficina/foundation.tfstate"
    region       = "us-east-1"
    encrypt      = true
    use_lockfile = true
  }
}