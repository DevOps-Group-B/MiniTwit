# ============================================================================
# Terraform Variables - Infrastructure Configuration
# ============================================================================

# Deployment mode: "local" for VirtualBox, "production" for DigitalOcean
variable "deployment_mode" {
  type        = string
  description = "Deployment environment: 'local' (VirtualBox) or 'production' (DigitalOcean)"
  default     = "local"
  
  validation {
    condition     = contains(["local", "production"], var.deployment_mode)
    error_message = "Deployment mode must be either 'local' or 'production'."
  }
}

# ============================================================================
# Common Infrastructure Variables
# ============================================================================

variable "project_name" {
  type        = string
  description = "Project name for resource naming and tagging"
  default     = "minitwit"
}

variable "environment" {
  type        = string
  description = "Environment tag (dev, staging, prod)"
  default     = "dev"
}

variable "domain_name" {
  type        = string
  description = "Domain name for the application"
  default     = "localhost"
}

variable "letsencrypt_email" {
  type        = string
  description = "Email for Let's Encrypt certificate registration"
  default     = "local@minitwit.test"
}

# ============================================================================
# Docker & Application Variables
# ============================================================================

variable "dockerhub_username" {
  type        = string
  description = "Docker Hub username for pulling images"
  default     = "sn3ke"
  sensitive   = true
}

variable "dockerhub_token" {
  type        = string
  description = "Docker Hub token for authentication"
  default     = ""
  sensitive   = true
}

variable "image_tag" {
  type        = string
  description = "Docker image tag to deploy"
  default     = "latest"
}

variable "postgres_connection_string" {
  type        = string
  description = "PostgreSQL connection string for the application"
  default     = ""
  sensitive   = true
}

variable "grafana_admin_user" {
  type        = string
  description = "Grafana administrator username"
  default     = "admin"
}

variable "grafana_admin_password" {
  type        = string
  description = "Grafana administrator password"
  default     = "admin"
  sensitive   = true
}

# ============================================================================
# Local Development (VirtualBox) Variables
# ============================================================================

variable "local_vm_name" {
  type        = string
  description = "Name of the local VirtualBox VM"
  default     = "MiniTwit-Local"
}

variable "local_vm_cpus" {
  type        = number
  description = "Number of CPU cores for local VM"
  default     = 2
  
  validation {
    condition     = var.local_vm_cpus >= 1 && var.local_vm_cpus <= 8
    error_message = "CPU count must be between 1 and 8."
  }
}

variable "local_vm_memory" {
  type        = number
  description = "Memory in MB for local VM"
  default     = 4096
  
  validation {
    condition     = var.local_vm_memory >= 2048 && var.local_vm_memory <= 16384
    error_message = "Memory must be between 2048 MB and 16384 MB."
  }
}

variable "local_host_port_http" {
  type        = number
  description = "Host port for HTTP traffic (guest 80)"
  default     = 8080
}

variable "local_host_port_https" {
  type        = number
  description = "Host port for HTTPS traffic (guest 443)"
  default     = 8443
}

variable "local_host_port_grafana" {
  type        = number
  description = "Host port for Grafana (guest 3000)"
  default     = 3000
}

variable "local_host_port_prometheus" {
  type        = number
  description = "Host port for Prometheus (guest 9090)"
  default     = 9090
}

variable "local_host_port_loki" {
  type        = number
  description = "Host port for Loki (guest 3100)"
  default     = 3100
}

variable "ubuntu_box_image" {
  type        = string
  description = "Ubuntu box image URL for local VirtualBox"
  default     = "Ubuntu22.04"
}

# ============================================================================
# Production (DigitalOcean) Variables
# ============================================================================

variable "digitalocean_token" {
  type        = string
  description = "DigitalOcean API token"
  default     = ""
  sensitive   = true
}

variable "digitalocean_region" {
  type        = string
  description = "DigitalOcean region for droplet deployment"
  default     = "sfo3"
}

variable "digitalocean_droplet_size" {
  type        = string
  description = "DigitalOcean droplet size"
  default     = "s-1vcpu-2gb"
}

variable "digitalocean_ssh_key_id" {
  type        = string
  description = "DigitalOcean SSH key ID for droplet access"
  default     = ""
}

variable "digitalocean_ssh_key_name" {
  type        = string
  description = "DigitalOcean SSH key name"
  default     = "MiniTwit"
}

variable "digitalocean_firewall_enable" {
  type        = bool
  description = "Enable firewall for DigitalOcean droplet"
  default     = true
}

variable "ssh_private_key_path" {
  type        = string
  description = "Path to SSH private key for authentication"
  default     = "~/.ssh/id_rsa"
}

variable "ssh_public_key_path" {
  type        = string
  description = "Path to SSH public key for DigitalOcean"
  default     = "~/.ssh/id_rsa.pub"
}

# ============================================================================
# Ansible Provisioning Variables
# ============================================================================

variable "ansible_playbook_path" {
  type        = string
  description = "Path to Ansible playbook for provisioning"
  default     = "../ansible/playbook.yml"
}

variable "ansible_inventory_path" {
  type        = string
  description = "Path to Ansible inventory file"
  default     = "../ansible/inventory.ini"
}

variable "provision_with_ansible" {
  type        = bool
  description = "Enable Ansible provisioning after infrastructure creation"
  default     = true
}

variable "ansible_verbose" {
  type        = bool
  description = "Enable verbose Ansible output"
  default     = false
}

# ============================================================================
# Tags and Labels
# ============================================================================

variable "common_tags" {
  type        = map(string)
  description = "Common tags to apply to all resources"
  default = {
    Project     = "MiniTwit"
    ManagedBy   = "Terraform"
    Environment = "development"
  }
}
