# ============================================================================
# Terraform Outputs
# ============================================================================
# Provides important information about deployed infrastructure

output "deployment_summary" {
  description = "Comprehensive deployment information"
  value = {
    deployment_mode = var.deployment_mode
    project_name    = var.project_name
    environment     = var.environment
    timestamp       = timestamp()
    
    # Local-specific outputs
    local = var.deployment_mode == "local" ? {
      vm_name     = var.local_vm_name
      cpus        = var.local_vm_cpus
      memory      = var.local_vm_memory
      http_port   = var.local_host_port_http
      grafana_port = var.local_host_port_grafana
    } : null

    # Production-specific outputs
    production = var.deployment_mode == "production" ? {
      droplet_id     = try(digitalocean_droplet.minitwit[0].id, "")
      droplet_ip     = try(digitalocean_droplet.minitwit[0].ipv4_address, "")
      reserved_ip    = try(digitalocean_reserved_ip.minitwit[0].ip_address, "")
      region         = var.digitalocean_region
      droplet_size   = var.digitalocean_droplet_size
      firewall_id    = try(digitalocean_firewall.minitwit[0].id, "")
    } : null
  }
}

output "access_urls" {
  description = "Application and monitoring access URLs"
  value = var.deployment_mode == "local" ? {
    web_app    = "http://localhost:${var.local_host_port_http}"
    grafana    = "http://localhost:${var.local_host_port_grafana}"
    prometheus = "http://localhost:${var.local_host_port_prometheus}"
    loki       = "http://localhost:${var.local_host_port_loki}"
  } : {
    web_app    = "https://${var.domain_name}"
    grafana    = "http://${try(digitalocean_droplet.minitwit[0].ipv4_address, "")}:3000"
    prometheus = "http://${try(digitalocean_droplet.minitwit[0].ipv4_address, "")}:9090"
    loki       = "http://${try(digitalocean_droplet.minitwit[0].ipv4_address, "")}:3100"
  }
}

output "ssh_connection" {
  description = "SSH connection information"
  value = var.deployment_mode == "local" ? 
    "ssh ubuntu@${try(libvirt_domain.minitwit_local[0].network_interface[0].addresses[0], 'localhost')}" :
    "ssh -i ${var.ssh_private_key_path} root@${try(digitalocean_droplet.minitwit[0].ipv4_address, '')}"
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
  value = var.deployment_mode == "local" ?
    [
      "1. Wait for cloud-init to complete: cloud-init status --wait",
      "2. Access the web app at: http://localhost:${var.local_host_port_http}",
      "3. Access Grafana at: http://localhost:${var.local_host_port_grafana}",
      "4. Login with: ${var.grafana_admin_user} / (see terraform variables)",
      "5. View logs: docker logs minitwit"
    ] :
    [
      "1. Wait for Ansible provisioning to complete (watch with: tail -f terraform.log)",
      "2. SSH into droplet: ssh -i ${var.ssh_private_key_path} root@${try(digitalocean_droplet.minitwit[0].ipv4_address, '')}",
      "3. Check docker status: docker ps",
      "4. Access via domain: https://${var.domain_name}",
      "5. View monitoring at: http://${try(digitalocean_droplet.minitwit[0].ipv4_address, '')}:3000"
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
