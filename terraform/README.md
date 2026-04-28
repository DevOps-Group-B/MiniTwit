# MiniTwit Infrastructure as Code (Terraform)

This directory contains Terraform configurations to manage MiniTwit infrastructure for both local development (VirtualBox) and production deployment (DigitalOcean).

## Directory Structure

```
terraform/
├── providers.tf                  # Provider configuration (Libvirt, DigitalOcean)
├── variables.tf                  # All input variables
├── main.tf                       # Main infrastructure definitions
├── local.tf                      # Local VirtualBox-specific resources
├── outputs.tf                    # Output definitions
├── terraform.tfvars.example      # Example variables file
├── templates/                    # Templates for cloud-init, inventory, etc.
│   ├── cloud-init-local.yml
│   ├── cloud-init-production.yml
│   ├── network-config.yml
│   ├── inventory-local.tpl
│   └── inventory-production.tpl
└── README.md                     # This file

.gitignore                        # Excludes terraform.tfstate, .terraform/, etc.
```

## Prerequisites

### All Deployments

1. **Terraform** (>= 1.0)
   ```bash
   # macOS
   brew install terraform
   
   # Linux
   curl -fsSL https://apt.releases.hashicorp.com/gpg | sudo apt-key add -
   sudo apt-add-repository "deb [arch=amd64] https://apt.releases.hashicorp.com $(lsb_release -cs) main"
   sudo apt-get update && sudo apt-get install terraform
   ```

2. **Ansible** (for provisioning)
   ```bash
   pip3 install ansible
   ansible-galaxy collection install community.docker
   ```

### Local Development (VirtualBox)

1. **Libvirt and QEMU/KVM** (Linux) or **VirtualBox** (macOS/Windows)
   ```bash
   # Ubuntu/Debian
   sudo apt-get install libvirt-daemon-system libvirt-clients qemu-system-x86-64
   
   # macOS (using Homebrew)
   brew install libvirt
   ```

2. **Terraform Libvirt Provider**
   - Automatically downloaded by `terraform init`
   - Requires: `dmacvicar/libvirt` plugin

### Production (DigitalOcean)

1. **DigitalOcean Account** with API token
   - Create at: https://cloud.digitalocean.com/account/api/tokens
   - Store securely (use environment variables)

2. **SSH Key** registered in DigitalOcean
   ```bash
   doctl compute ssh-key list
   ```

## Quick Start

### Local Development (VirtualBox)

```bash
# 1. Initialize Terraform
terraform init

# 2. Create terraform.tfvars from example
cp terraform.tfvars.example terraform.tfvars

# 3. Edit for local deployment
# - Set deployment_mode = "local"
# - Adjust CPU/memory as needed

# 4. Plan infrastructure changes
terraform plan

# 5. Apply configuration
terraform apply

# 6. Monitor deployment
# Watch cloud-init: cloud-init status --watch

# 7. Access services
# - Web App: http://localhost:8080
# - Grafana: http://localhost:3000
# - Prometheus: http://localhost:9090
# - Loki: http://localhost:3100
```

### Production (DigitalOcean)

```bash
# 1. Set DigitalOcean token
export TF_VAR_digitalocean_token="dop_v1_xxx"

# 2. Initialize Terraform
terraform init

# 3. Create terraform.tfvars
cp terraform.tfvars.example terraform.tfvars

# 4. Edit for production
# - Set deployment_mode = "production"
# - Set digitalocean_token in tfvars or via environment
# - Configure domain_name, email, region, etc.

# 5. List available SSH keys
doctl compute ssh-key list
# Copy the SSH key ID to terraform.tfvars

# 6. Plan infrastructure
terraform plan

# 7. Apply configuration
terraform apply

# 8. Wait for provisioning
# Monitor: tail -f terraform.log

# 9. Get droplet information
terraform output deployment_summary

# 10. SSH into droplet
ssh -i ~/.ssh/id_rsa root@<DROPLET_IP>
```

## Configuration

### Key Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `deployment_mode` | `local` | `local` (VirtualBox) or `production` (DigitalOcean) |
| `project_name` | `minitwit` | Project identifier |
| `environment` | `dev` | Environment tag |
| `domain_name` | `localhost` | Domain for application |

### Environment Variables (Sensitive)

```bash
export TF_VAR_digitalocean_token="your_token"
export TF_VAR_postgres_connection_string="Host=..."
export TF_VAR_dockerhub_token="xxx"
export TF_VAR_grafana_admin_password="secure_password"
```

### Customization

Edit `terraform.tfvars` to customize:
- VM resources (CPU, memory)
- Port mappings
- Deployment region
- Monitoring credentials
- Application configuration

## Deployment Flow

### Local (VirtualBox)

```
Terraform Plan
    ↓
Create Disk Volume
    ↓
Generate Cloud-Init Config
    ↓
Create Virtual Machine
    ↓
Boot VM with Cloud-Init
    ↓
Cloud-Init: Install Docker, Docker Compose, Ansible
    ↓
Ansible Playbook Runs
    ↓
Containers Start (App, Postgres, Prometheus, Grafana, Loki, etc.)
```

### Production (DigitalOcean)

```
Terraform Plan
    ↓
Validate SSH Keys
    ↓
Create Droplet (Ubuntu 22.04)
    ↓
Droplet Boots
    ↓
Remote-Exec: Install Python, Ansible
    ↓
Generate Ansible Inventory
    ↓
Ansible Provisioning
    ↓
Docker Stack Deploys
    ↓
DNS/SSL Configuration
    ↓
Containers Running
```

## Common Commands

```bash
# Initialize working directory
terraform init

# Format configuration files
terraform fmt -recursive

# Validate configuration
terraform validate

# Plan changes (dry-run)
terraform plan

# Apply changes
terraform apply

# Show current state
terraform show

# Destroy infrastructure (CAREFUL!)
terraform destroy

# Get specific output
terraform output deployment_summary

# Refresh state
terraform refresh

# Debug mode
TF_LOG=DEBUG terraform plan
```

## State Management

### Local State (Development)

```bash
# Current setup: terraform.tfstate in working directory
# WARNING: Not safe for teams or production!
# .gitignore should exclude: terraform.tfstate
```

### Remote State (Recommended for Production)

Configure backend in `providers.tf`:

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

Or use Terraform Cloud:

```hcl
terraform {
  cloud {
    organization = "your-org"
    workspaces {
      name = "minitwit-production"
    }
  }
}
```

## Troubleshooting

### SSH Connection Errors

```bash
# Verify SSH key permissions
chmod 600 ~/.ssh/id_rsa

# Test SSH manually
ssh -i ~/.ssh/id_rsa ubuntu@<VM_IP>

# Check SSH config
cat ~/.ssh/config
```

### Cloud-Init Hangs

```bash
# SSH into VM and check status
cloud-init status

# View cloud-init logs (local VM)
sudo cat /var/log/cloud-init-output.log
```

### Ansible Provisioning Fails

```bash
# Run ansible manually with verbose output
ansible-playbook -i terraform/inventory.ini -vvv ../ansible/playbook.yml

# Check inventory file
cat terraform/inventory.ini
```

### DigitalOcean SSH Key Not Found

```bash
# List available keys
doctl compute ssh-key list

# Copy the ID
# Set in terraform.tfvars: digitalocean_ssh_key_id = "xxxxx"
```

## Cleanup

### Destroy All Infrastructure

```bash
# Plan destruction
terraform plan -destroy

# Actually destroy
terraform destroy

# Confirm when prompted
```

### Keep State, Destroy Infrastructure

```bash
# Useful for debugging
terraform destroy -auto-approve
```

## Security Best Practices

1. **Never commit sensitive data**
   - `.gitignore` should include: `terraform.tfvars`, `*.pem`, `*.key`
   - Use environment variables for secrets

2. **Use remote state with encryption**
   - Enable encryption at rest (S3, Azure, etc.)
   - Enable state locking

3. **Regular backups**
   - Backup `terraform.tfstate`
   - Backup SSH keys

4. **Firewall rules**
   - DigitalOcean firewall configured by default
   - SSH limited to authorized keys only

5. **Rotate credentials regularly**
   - API tokens
   - SSH keys
   - Database passwords

## Advanced Usage

### Multiple Environments

```bash
# Development
terraform apply -var-file=dev.tfvars

# Staging
terraform apply -var-file=staging.tfvars

# Production
terraform apply -var-file=prod.tfvars
```

### Workspaces

```bash
# Create separate workspaces
terraform workspace new dev
terraform workspace new prod

# Switch workspaces
terraform workspace select prod

# List workspaces
terraform workspace list
```

### Import Existing Resources

```bash
# Import existing DigitalOcean droplet
terraform import digitalocean_droplet.minitwit <DROPLET_ID>

# Verify import
terraform plan
```

## Resources

- [Terraform Documentation](https://www.terraform.io/docs)
- [DigitalOcean Provider](https://registry.terraform.io/providers/digitalocean/digitalocean/latest/docs)
- [Libvirt Provider](https://registry.terraform.io/providers/dmacvicar/libvirt/latest/docs)
- [Ansible Documentation](https://docs.ansible.com/)

## Support

For issues or questions:
1. Check Terraform logs: `TF_LOG=DEBUG terraform plan`
2. Review cloud-init logs (local) or /var/log/syslog (DigitalOcean)
3. Verify SSH connectivity
4. Check Ansible inventory and playbook

## License

Same as MiniTwit project
