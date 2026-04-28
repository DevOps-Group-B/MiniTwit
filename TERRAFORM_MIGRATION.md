# Infrastructure as Code - Migration Guide (Vagrant → Terraform)

## Overview

MiniTwit infrastructure has been converted from **Vagrant** (Ruby-based provisioning tool) to **Terraform/OpenTofu** (HCL-based Infrastructure as Code). This migration provides:

- **Better cloud support** - DigitalOcean, AWS, Azure, GCP, etc.
- **State management** - Track infrastructure state explicitly
- **Reproducibility** - Same HCL for any environment
- **Team collaboration** - Easier to review and manage via VCS
- **Multi-cloud** - Easily switch or use multiple providers
- **Scaling** - Better support for multiple instances and environments

## Key Differences

### Vagrant (Old)
- DSL: Ruby
- Focus: Local VM management
- State: Implicit (in .vagrant/)
- Provisioner: Ansible via Vagrant plugin
- Multi-provider: Limited (plugin-based)

### Terraform (New)
- DSL: HCL (HashiCorp Configuration Language)
- Focus: Infrastructure definition across any provider
- State: Explicit (terraform.tfstate)
- Provisioner: Terraform native + remote-exec
- Multi-provider: First-class support

## Architecture Comparison

### Vagrant Flow
```
Vagrantfile (Ruby)
    ↓
Vagrant Commands (vagrant up/destroy)
    ↓
Provider Setup (VirtualBox/DigitalOcean plugin)
    ↓
Ansible Provisioning
    ↓
Running Infrastructure
```

### Terraform Flow
```
Terraform HCL Files
    ├── providers.tf (provider configuration)
    ├── variables.tf (input variables)
    ├── main.tf (main resources)
    ├── local.tf (local-specific)
    └── outputs.tf (outputs)
    ↓
Terraform Commands (terraform plan/apply)
    ↓
Remote API Calls
    ↓
Cloud Provider (VirtualBox/DigitalOcean)
    ↓
Cloud-Init Initialization
    ↓
Ansible Provisioning
    ↓
Running Infrastructure (with state tracking)
```

## Migration Steps

### 1. Install Terraform

```bash
# macOS
brew install terraform

# Ubuntu/Debian
curl -fsSL https://apt.releases.hashicorp.com/gpg | sudo apt-key add -
sudo apt-add-repository "deb [arch=amd64] https://apt.releases.hashicorp.com $(lsb_release -cs) main"
sudo apt-get update && sudo apt-get install terraform

# Verify
terraform version
```

### 2. Stop and Backup Existing Vagrant Infrastructure

```bash
# If you have running Vagrant VMs
cd /path/to/MiniTwit
vagrant global-status

# Halt Vagrant
vagrant halt

# Backup state
vagrant global-status > vagrant-backup.txt
```

### 3. Initialize Terraform

```bash
cd /path/to/MiniTwit/terraform
terraform init
cp terraform.tfvars.example terraform.tfvars

# Edit configuration
nano terraform.tfvars
# Set: deployment_mode = "local" (or "production")
```

### 4. Plan and Review Changes

```bash
# See what will be created
terraform plan

# Save plan for review
terraform plan -out=tfplan
```

### 5. Apply Terraform Configuration

```bash
# Apply the plan
terraform apply tfplan

# Or apply directly
terraform apply -auto-approve
```

### 6. Verify Deployment

```bash
# Check outputs
terraform output deployment_summary
terraform output access_urls

# Check containers
docker ps

# Check logs
docker logs minitwit
```

### 7. Remove Vagrant (when confident)

```bash
# If Terraform deployment is successful
vagrant destroy

# Delete Vagrant files
rm -rf .vagrant/
rm ../Vagrantfile  # Or keep as reference

# Git cleanup
git rm -f ../Vagrantfile
git add ../
git commit -m "chore: migrate from Vagrant to Terraform"
```

## File Mapping

### Vagrant → Terraform Equivalents

| Vagrant | Terraform | Purpose |
|---------|-----------|---------|
| `Vagrantfile` | `terraform/*.tf` | Infrastructure definition |
| `Vagrant.configure()` | `providers.tf` | Provider setup |
| `config.vm.box` | `libvirt_volume` + `libvirt_domain` | VM specification |
| `machine_config.vm.provision` | `provisioner "remote-exec"` + `null_resource` | Provisioning |
| Environment variables | `variables.tf` + `terraform.tfvars` | Configuration |
| Output messages | `outputs.tf` | Information display |

## Configuration Comparison

### Vagrant (Ruby)

```ruby
# Vagrantfile
config.vm.define "default" do |local|
  local.vm.box = "ubuntu/jammy64"
  local.vm.provider "virtualbox" do |vb|
    vb.memory = 4096
    vb.cpus = 2
  end
  
  local.vm.provision "ansible_local" do |ansible|
    ansible.playbook = "ansible/playbook.yml"
  end
end
```

### Terraform (HCL)

```hcl
# terraform/local.tf
resource "libvirt_domain" "minitwit_local" {
  count   = var.deployment_mode == "local" ? 1 : 0
  name    = var.local_vm_name
  memory  = var.local_vm_memory
  vcpu    = var.local_vm_cpus
  running = true
  # ... additional configuration
}
```

## Usage Comparison

### Vagrant Commands vs Terraform Commands

| Task | Vagrant | Terraform |
|------|---------|-----------|
| **Initialize** | `vagrant init` | `terraform init` |
| **View configuration** | `vagrant status` | `terraform show` |
| **Plan changes** | `vagrant validate` | `terraform plan` |
| **Create infrastructure** | `vagrant up` | `terraform apply` |
| **Update infrastructure** | `vagrant provision` | `terraform apply` |
| **Destroy infrastructure** | `vagrant destroy` | `terraform destroy` |
| **SSH into VM** | `vagrant ssh` | `ssh ubuntu@<IP>` |

## Provider-Specific Changes

### Local Development (VirtualBox)

**Vagrant:**
```ruby
local.vm.provider "virtualbox" do |vb|
  vb.memory = 4096
end
```

**Terraform:**
```hcl
# Uses libvirt/KVM (Linux) instead
resource "libvirt_domain" "minitwit_local" {
  memory = var.local_vm_memory
}
```

### Production (DigitalOcean)

**Vagrant:**
```ruby
production.vm.provider :digitalocean do |provider|
  provider.token = ENV.fetch("DIGITALOCEAN_TOKEN")
end
```

**Terraform:**
```hcl
provider "digitalocean" {
  token = var.digitalocean_token
}

resource "digitalocean_droplet" "minitwit" {
  region = var.digitalocean_region
  image  = "ubuntu-22-04-x64"
}
```

## State Management

### Vagrant State (Implicit)

```
.vagrant/
├── machines/
│   └── default/
│       └── virtualbox/
│           └── id  # VM ID
└── provisioners/
```

### Terraform State (Explicit)

```
terraform.tfstate          # Current state
terraform.tfstate.backup   # Previous state (auto-created)
.terraform/                # Provider plugins and modules
```

## Migrating to Remote State (Recommended for Production)

### S3 Backend

```hcl
# terraform/providers.tf
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

### Terraform Cloud

```hcl
# terraform/providers.tf
terraform {
  cloud {
    organization = "your-org"
    workspaces {
      name = "minitwit"
    }
  }
}
```

## Troubleshooting Migration

### Issue: "Provider not found"

```bash
# Solution: Ensure providers are installed
cd terraform
terraform init -upgrade
```

### Issue: "Resource address not found"

```bash
# Solution: Check resource names match exactly
terraform state list

# If importing existing resources:
terraform import digitalocean_droplet.minitwit <DROPLET_ID>
```

### Issue: "State file corrupted"

```bash
# Keep manual backup
cp terraform.tfstate terraform.tfstate.backup

# Try to revert to backup
cp terraform.tfstate.backup terraform.tfstate

# Check state validity
terraform state list
```

### Issue: "Port conflicts"

```hcl
# terraform/terraform.tfvars
local_host_port_http = 8081  # Change if 8080 is in use
```

## Rollback Plan

### If Terraform Deployment Fails

```bash
# 1. Examine the error
terraform apply 2>&1 | tee error.log

# 2. Check Terraform state
terraform show

# 3. Destroy what was partially created
terraform destroy

# 4. Fix configuration
# - Edit *.tf files
# - Check variables

# 5. Try again
terraform plan
terraform apply
```

### Fallback to Vagrant

If you need to immediately fall back:

```bash
# Terraform will have created infrastructure
# But Vagrant metadata is gone, so:

vagrant up  # This will fail or create new VM

# Instead, manually access the created infrastructure
# Or import into Vagrant manually (not recommended)
```

## Best Practices

1. **Version Control**
   - Commit all .tf files
   - Exclude .tfstate from git
   - Use .gitignore (provided in terraform/)

2. **State Management**
   - Never edit terraform.tfstate manually
   - Always backup before major changes
   - Use remote state for team environments

3. **Sensitive Data**
   - Use environment variables for secrets
   - Never commit terraform.tfvars with secrets
   - Use Terraform Cloud's encrypted variables

4. **Code Organization**
   - Separate concerns (local.tf, main.tf, etc.)
   - Use variables for configuration
   - Document complex resources

5. **Testing**
   - Run `terraform validate`
   - Run `terraform plan` before apply
   - Test in non-production first

## Further Reading

- [Terraform Documentation](https://www.terraform.io/docs)
- [Terraform Best Practices](https://www.terraform.io/language)
- [From Vagrant to Terraform](https://www.terraform.io/guides/alternative-providers/vagrant)
- [OpenTofu (CNCF Fork)](https://opentofu.org/)

## Quick Reference

```bash
# Initialize
terraform init

# Format
terraform fmt -recursive .

# Validate
terraform validate

# Plan
terraform plan -var-file=terraform.tfvars

# Apply
terraform apply -var-file=terraform.tfvars

# Destroy
terraform destroy -var-file=terraform.tfvars

# State
terraform state list
terraform state show <resource>

# Debug
TF_LOG=DEBUG terraform plan
```
