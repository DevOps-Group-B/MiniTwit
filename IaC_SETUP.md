# Infrastructure as Code - Complete Setup Guide

This guide covers the complete Infrastructure-as-Code (IaC) setup for MiniTwit, including both legacy Vagrant and modern Terraform/OpenTofu configurations.

## Overview

Your infrastructure is now managed entirely as code in two ways:

### 🚀 **Primary Method: Terraform (Recommended)**
- **Language**: HCL (HashiCorp Configuration Language)
- **Providers**: VirtualBox (local), DigitalOcean (production)
- **State Management**: Explicit tracking via `terraform.tfstate`
- **Location**: `terraform/` directory

### 📦 **Legacy Method: Vagrant**
- **Language**: Ruby DSL
- **Status**: Supported but consider migrating to Terraform
- **Location**: `Vagrantfile` (root)

## Quick Start

### Using Terraform (Recommended)

```bash
# 1. Initialize Terraform (one-time)
cd /path/to/MiniTwit/terraform
terraform init

# 2. Copy and edit configuration
cp terraform.tfvars.example terraform.tfvars
nano terraform.tfvars  # Edit for your setup

# 3. Plan changes
terraform plan

# 4. Deploy
terraform apply

# 5. Access your services
# Web App:    http://localhost:8080
# Grafana:    http://localhost:3000
# Prometheus: http://localhost:9090
# Loki:       http://localhost:3100
```

### Using Vagrant (Legacy)

```bash
# 1. Start local VM
vagrant up

# 2. Wait for provisioning
# ... (may take 5-10 minutes)

# 3. Access your services
# Same URLs as Terraform above
```

## Directory Structure

```
MiniTwit/
├── terraform/                    # Terraform Infrastructure as Code
│   ├── providers.tf              # Provider configuration
│   ├── variables.tf              # Input variables
│   ├── main.tf                   # Main infrastructure
│   ├── local.tf                  # VirtualBox-specific (optional)
│   ├── outputs.tf                # Output values
│   ├── terraform.tfvars.example  # Example configuration
│   ├── README.md                 # Terraform documentation
│   ├── .gitignore                # Git ignore rules
│   └── templates/                # Cloud-init and Ansible templates
│       ├── cloud-init-local.yml
│       ├── cloud-init-production.yml
│       ├── network-config.yml
│       ├── inventory-local.tpl
│       └── inventory-production.tpl
├── Vagrantfile                   # Vagrant config (legacy - optional)
├── ansible/                      # Ansible playbooks
│   ├── playbook.yml
│   ├── inventory.ini
│   └── roles/
├── docker-compose.yml            # Docker Compose config
├── TERRAFORM_MIGRATION.md        # Vagrant → Terraform migration guide
└── IaC_SETUP.md                  # This file
```

## Configuration Files

### Terraform Configuration

#### `terraform/variables.tf`
Defines all input variables:
- Deployment mode (local/production)
- VM resources (CPU, memory)
- Docker/application settings
- DigitalOcean credentials
- Port mappings

#### `terraform/terraform.tfvars`
Provides values for variables (create from example):
```bash
cp terraform/terraform.tfvars.example terraform/terraform.tfvars
```

Example values:
```hcl
deployment_mode = "local"
project_name = "minitwit"
local_vm_memory = 4096
domain_name = "localhost"
```

### Ansible Configuration

Ansible playbook runs after Terraform/Vagrant creates VMs:
- **Location**: `ansible/playbook.yml`
- **Roles**: `ansible/roles/minitwit/`
- **Inventory**: Auto-generated from Terraform

#### Ansible Responsibilities
1. Install Docker and Docker Compose
2. Pull/build Docker images
3. Start containers (app, database, monitoring)
4. Configure volumes and networks
5. Setup logging and monitoring

## Deployment Modes

### Local Development

**Start**:
```bash
cd terraform
terraform init
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars: set deployment_mode = "local"
terraform apply
```

### Production Deployment (DigitalOcean)

## Environment Configuration

### Local Development

Create `terraform/terraform.tfvars`:
```hcl
deployment_mode = "local"
domain_name = "localhost"
grafana_admin_password = "your-secure-password"
```

### Production

Create `terraform/terraform.tfvars`:
```hcl
deployment_mode = "production"
domain_name = "minitwit.tech"
letsencrypt_email = "your@email.com"
digitalocean_region = "sfo3"
```

Set sensitive values via environment:
```bash
export TF_VAR_digitalocean_token="dop_v1_..."
export TF_VAR_postgres_connection_string="Host=..."
export TF_VAR_grafana_admin_password="..."
```

## Common Commands

```bash
cd terraform

# Initialize Terraform (first time only)
terraform init

# Format code
terraform fmt -recursive .

# Validate config
terraform validate

# Show what will be created/destroyed
terraform plan

# Deploy infrastructure
terraform apply

# Show current infrastructure
terraform show

# Destroy infrastructure (be careful!)
terraform destroy

# Get specific output
terraform output deployment_summary
terraform output access_urls
```

## Basic workflow

```bash
cd terraform

# Initialize once
terraform init

# Review your config
terraform plan

# Deploy
terraform apply

# Check status
terraform show

# Destroy (if needed)
terraform destroy
```

## State Management

### Local State (Development)

Default setup stores state locally in `terraform.tfstate`:

```
terraform/
├── terraform.tfstate          # Current state (JSON)
├── terraform.tfstate.backup   # Previous state
└── .terraform/                # Provider cache
```

### Remote State (Production - Recommended)

Configure in `terraform/providers.tf`:

**S3 Backend** (AWS):
```hcl
terraform {
  backend "s3" {
    bucket         = "minitwit-terraform-state"
    key            = "minitwit/terraform.tfstate"
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "terraform-locks"
  }
}
```

**Terraform Cloud** (Recommended):
```hcl
terraform {
  cloud {
    organization = "your-org"
    workspaces {
      name = "minitwit"
    }
  }
}
```

Enable remote state:
```bash
terraform init  # Re-initialize with backend config
terraform apply  # Starts using remote state
```

## Security Considerations

### Secrets Management

**Never commit secrets!**

Use environment variables:
```bash
export TF_VAR_digitalocean_token="..."
export TF_VAR_postgres_connection_string="..."
export TF_VAR_grafana_admin_password="..."
```

Or use `.env` file (add to `.gitignore`):
```bash
cp .env.example .env
source .env
terraform apply
```

### SSH Keys

Store securely:
```bash
chmod 600 ~/.ssh/id_rsa
# Never commit private keys!
```

### Terraform State

The state file contains sensitive data:
```bash
# Backup regularly
cp terraform.tfstate terraform.tfstate.backup

# Use remote state with encryption (production)
# Never commit to git (in .gitignore)

# Use sensit Terraform variables
variable "database_password" {
  sensitive = true
}
```

## Troubleshooting

```bash
# Validate configuration
terraform validate

# Check Terraform version
terraform version

# Clear and reinitialize
rm -rf .terraform/
terraform init -upgrade

# Debug mode
TF_LOG=DEBUG terraform plan

# Check what Terraform will do
terraform plan -out=tfplan

# Emergency rollback
terraform destroy -auto-approve  # ⚠️ Use with caution!
```

## Migration from Vagrant

If you're coming from Vagrant:

1. Stop Vagrant VMs: `vagrant halt`
2. Initialize Terraform: `cd terraform && terraform init`
3. Create config: `cp terraform.tfvars.example terraform.tfvars`
4. Deploy: `terraform apply`
5. When ready, remove Vagrant: `rm Vagrantfile && vagrant destroy`

See [TERRAFORM_MIGRATION.md](./TERRAFORM_MIGRATION.md) for details.

## Support

```bash
# Get help
terraform help
terraform help plan
terraform help apply

# Detailed logging
TF_LOG=DEBUG terraform plan

# Check state
terraform state list
terraform state show <resource>
```

---

**Quick Reference**:
```bash
cd terraform
terraform init       # First time only
terraform plan       # Preview changes
terraform apply      # Deploy
terraform destroy    # Tear down
terraform fmt .      # Format code
```

For more info, see [Terraform README](./terraform/README.md) or [Migration Guide](./TERRAFORM_MIGRATION.md).
