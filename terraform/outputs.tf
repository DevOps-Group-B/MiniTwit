# ============================================================================
# Terraform Outputs
# ============================================================================
# Provides important information about deployed infrastructure

output "deployment_summary" {
  description = "Comprehensive deployment information"
  value = {
    deployment_mode = "production"
    project_name    = var.project_name
    environment     = var.environment
    timestamp       = timestamp()

    # Production-specific outputs
    production = {
      droplet_id   = digitalocean_droplet.minitwit.id
      droplet_ip   = digitalocean_droplet.minitwit.ipv4_address
      region       = var.digitalocean_region
      droplet_size = var.digitalocean_droplet_size
      firewall_id  = try(digitalocean_firewall.minitwit[0].id, "")
    }
  }
}

output "access_urls" {
  description = "Application and monitoring access URLs"
  value = {
    web_app    = "https://${var.domain_name}"
    grafana    = "http://${digitalocean_droplet.minitwit.ipv4_address}:3000"
    prometheus = "http://${digitalocean_droplet.minitwit.ipv4_address}:9090"
    loki       = "http://${digitalocean_droplet.minitwit.ipv4_address}:3100"
  }
}

output "ssh_connection" {
  description = "SSH connection information"
  value = "ssh -i ${var.ssh_private_key_path} root@${digitalocean_droplet.minitwit.ipv4_address}"
}

output "grafana_credentials" {
  description = "Grafana default credentials"
  value = {
    username = var.grafana_admin_user
    password = "***REDACTED***"  # For security, actual password not shown
  }
  sensitive = true
}

output "next_steps" {
  description = "Next steps after infrastructure deployment"
  value = [
    "1. Wait for Ansible provisioning to complete (watch with: tail -f terraform.log)",
    "2. SSH into droplet: ssh -i ${var.ssh_private_key_path} root@${digitalocean_droplet.minitwit.ipv4_address}",
    "3. Check docker status: docker ps",
    "4. Access via domain: https://${var.domain_name}",
    "5. View monitoring at: http://${digitalocean_droplet.minitwit.ipv4_address}:3000"
  ]
}

output "terraform_state_info" {
  description = "Information about Terraform state management"
  value = {
    state_file = "terraform.tfstate (local) - consider migrating to remote backend"
    recommended_backend = [
      "S3 for AWS",
      "Azure Blob Storage for Azure",
      "Terraform Cloud for managed state",
      "Local backend for development"
    ]
    docs = "See README.md for state management setup"
  }
}
