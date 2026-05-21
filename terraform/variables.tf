# ============================================================================
# Terraform Variables - Production Infrastructure Configuration
# ============================================================================

variable "project_name" {
  type        = string
  description = "Project name for resource naming and tagging"
  default     = "minitwit"
}

variable "environment" {
  type        = string
  description = "Environment tag (dev, staging, prod)"
  default     = "prod"
}

variable "domain_name" {
  type        = string
  description = "Domain name for the application"
  default     = "minitwit.tech"
}

variable "letsencrypt_email" {
  type        = string
  description = "Email for Let's Encrypt certificate registration"
  default     = ""
}

variable "keepalived_auth_pass" {
  type        = string
  description = "Keepalived VRRP auth password (must be 1-8 characters)"
  sensitive   = true

  validation {
    condition     = length(var.keepalived_auth_pass) > 0 && length(var.keepalived_auth_pass) <= 8
    error_message = "keepalived_auth_pass must be 1-8 characters because keepalived auth_pass is limited to 8 characters."
  }
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

variable "db_password" {
  type        = string
  description = "Database password"
  default     = ""
  sensitive   = true
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
  default     = ""
}

variable "grafana_admin_password" {
  type        = string
  description = "Grafana administrator password"
  default     = ""
  sensitive   = true
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
  default     = "fra1"
}

variable "digitalocean_droplet_size" {
  type        = string
  description = "DigitalOcean droplet size"
  default     = "s-1vcpu-2gb"
}

variable "digitalocean_db_droplet_size" {
  type        = string
  description = "DigitalOcean droplet size for the database node"
  default     = "s-1vcpu-1gb"
}

variable "digitalocean_ssh_key_name" {
  type        = string
  description = "DigitalOcean SSH key name"
  default     = "insert your own key"
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

# ============================================================================
# Ansible Provisioning Variables
# ============================================================================

variable "ansible_playbook_path" {
  type        = string
  description = "Path to Ansible playbook for provisioning"
  default     = "../ansible/playbook.yml"
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
    Environment = "prod"
  }
}