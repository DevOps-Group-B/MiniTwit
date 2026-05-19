# ============================================================================
# Terraform Outputs
# ============================================================================

output "deployment_summary" {
  description = "Comprehensive deployment information for HA app/LB and DB nodes"
  value = {
    deployment_mode = "production-ha"
    project_name    = var.project_name
    environment     = var.environment
    timestamp       = timestamp()

    lb_primary = {
      droplet_id  = digitalocean_droplet.minitwit_lb_primary.id
      droplet_ip  = digitalocean_droplet.minitwit_lb_primary.ipv4_address
      firewall_id = digitalocean_firewall.web.id
    }

    lb_secondary = {
      droplet_id  = digitalocean_droplet.minitwit_lb_secondary.id
      droplet_ip  = digitalocean_droplet.minitwit_lb_secondary.ipv4_address
      firewall_id = digitalocean_firewall.web.id
    }

    database_server = {
      droplet_id  = digitalocean_droplet.database.id
      droplet_ip  = digitalocean_droplet.database.ipv4_address
      firewall_id = digitalocean_firewall.database.id
    }

    floating_ip = digitalocean_floating_ip.minitwit.ip_address
    region      = var.digitalocean_region
    web_size    = var.digitalocean_droplet_size
    db_size     = var.digitalocean_db_droplet_size
  }
}

output "access_urls" {
  description = "Application and monitoring access URLs (via floating IP and domain)"
  value = {
    web_app        = "https://${var.domain_name}"
    web_app_direct = "http://${digitalocean_floating_ip.minitwit.ip_address}"
    grafana        = "http://${digitalocean_floating_ip.minitwit.ip_address}:3000"
    prometheus     = "http://${digitalocean_floating_ip.minitwit.ip_address}:9090"
    loki           = "http://${digitalocean_floating_ip.minitwit.ip_address}:3100"
  }
}

output "ssh_connection_commands" {
  description = "SSH connection strings for HA and DB nodes"
  value = {
    lb_primary   = "ssh -i ${var.ssh_private_key_path} root@${digitalocean_droplet.minitwit_lb_primary.ipv4_address}"
    lb_secondary = "ssh -i ${var.ssh_private_key_path} root@${digitalocean_droplet.minitwit_lb_secondary.ipv4_address}"
    db_node      = "ssh -i ${var.ssh_private_key_path} root@${digitalocean_droplet.database.ipv4_address}"
  }
}

output "grafana_credentials" {
  description = "Grafana default credentials"
  value = {
    username = var.grafana_admin_user
    password = "***REDACTED***"
  }
  sensitive = true
}

output "next_steps" {
  description = "Post-deployment verification steps"
  value = [
    "1. Wait for Ansible to finish.",
    "2. Verify keepalived: ssh into both LB nodes and run 'sudo systemctl status keepalived'.",
    "3. Verify floating IP ownership on primary: 'ip addr | grep ${digitalocean_floating_ip.minitwit.ip_address}'.",
    "4. Verify DB connectivity from LB node: run the app and check DB logs.",
  ]
}

output "terraform_state_info" {
  description = "State management reminder"
  value = {
    state_file = "terraform.tfstate (local)"
    notice     = "Ensure this file is backed up or migrate to a remote backend for team collaboration."
  }
}
