# ============================================================================
# Terraform Outputs
# ============================================================================
# Provides important information about the distributed multi-node infrastructure

output "deployment_summary" {
  description = "Comprehensive deployment information for Web and DB nodes"
  value = {
    deployment_mode = "production"
    project_name    = var.project_name
    environment     = var.environment
    timestamp       = timestamp()

    # Web Server Details
    web_server = {
      droplet_id   = digitalocean_droplet.minitwit.id
      droplet_ip   = digitalocean_droplet.minitwit.ipv4_address
      firewall_id  = digitalocean_firewall.web.id
    }

    # Database Server Details
    database_server = {
      droplet_id   = digitalocean_droplet.database.id
      droplet_ip   = digitalocean_droplet.database.ipv4_address
      firewall_id  = digitalocean_firewall.database.id
    }

    region       = var.digitalocean_region
    common_size  = var.digitalocean_droplet_size
  }
}

output "access_urls" {
  description = "Application and monitoring access URLs (hosted on Web node)"
  value = {
    web_app    = "https://${var.domain_name}"
    grafana    = "http://${digitalocean_droplet.minitwit.ipv4_address}:3000"
    prometheus = "http://${digitalocean_droplet.minitwit.ipv4_address}:9090"
    loki       = "http://${digitalocean_droplet.minitwit.ipv4_address}:3100"
  }
}

output "ssh_connection_commands" {
  description = "SSH connection strings for both droplets"
  value = {
    web_node = "ssh -i ${var.ssh_private_key_path} root@${digitalocean_droplet.minitwit.ipv4_address}"
    db_node  = "ssh -i ${var.ssh_private_key_path} root@${digitalocean_droplet.database.ipv4_address}"
  }
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
  description = "Post-deployment verification steps"
  value = [
    "1. Wait for Ansible to finish (Monitor with: tail -f terraform.log or watch the terminal)",
    "2. Verify DB: ssh into DB node and run 'sudo -u postgres psql -c \"\\dt\" minitwit'",
    "3. Verify Web: ssh into Web node and run 'docker ps'",
    "4. Check Site: https://${var.domain_name}",
    "5. Check Metrics: http://${digitalocean_droplet.minitwit.ipv4_address}:3000"
  ]
}

output "terraform_state_info" {
  description = "State management reminder"
  value = {
    state_file = "terraform.tfstate (local)"
    notice     = "Ensure this file is backed up or migrate to a remote backend (S3/DO Spaces) for team collaboration."
  }
}