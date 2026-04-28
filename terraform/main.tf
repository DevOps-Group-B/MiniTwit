# ============================================================================
# MiniTwit Infrastructure as Code - Main Configuration
# ============================================================================
# This configuration manages both local (VirtualBox) and production 
# (DigitalOcean) infrastructure deployment

locals {
  common_tags = merge(
    var.common_tags,
    {
      Project       = var.project_name
      ManagedBy     = "Terraform"
      DeploymentMode = var.deployment_mode
      CreatedAt     = timestamp()
    }
  )
}

# ============================================================================
# Local Development Infrastructure (VirtualBox)
# ============================================================================

# Generate cloud-init script for VM initialization
resource "local_file" "cloud_init_script" {
  count    = var.deployment_mode == "local" ? 1 : 0
  filename = "${path.module}/cloud-init-${var.deployment_mode}.yml"

  content = templatefile("${path.module}/templates/cloud-init.tpl", {
    dockerhub_username = var.dockerhub_username
    image_tag          = var.image_tag
  })

  lifecycle {
    ignore_changes = [content]
  }
}

# ============================================================================
# Production Infrastructure (DigitalOcean)
# ============================================================================

# Fetch DigitalOcean SSH key for droplet access
data "digitalocean_ssh_key" "default" {
  count = var.deployment_mode == "production" ? 1 : 0
  name  = var.digitalocean_ssh_key_name
}

# DigitalOcean Droplet (VPS)
resource "digitalocean_droplet" "minitwit" {
  count              = var.deployment_mode == "production" ? 1 : 0
  name               = "${var.project_name}-${var.environment}-droplet"
  region             = var.digitalocean_region
  size               = var.digitalocean_droplet_size
  image              = "ubuntu-22-04-x64"
  backups            = true
  ipv6               = true
  private_networking = true
  monitoring         = true

  ssh_keys = [data.digitalocean_ssh_key.default[0].id]

  tags = [var.project_name, var.environment]

  # Provision the instance after creation
  provisioner "remote-exec" {
    inline = [
      "sleep 30",  # Wait for SSH to become available
      "sudo apt-get update",
      "sudo apt-get install -y python3-pip python3-venv git curl",
      "pip3 install --upgrade ansible",
      "ansible-galaxy collection install community.docker"
    ]

    connection {
      type        = "ssh"
      user        = "root"
      private_key = file(pathexpand(var.ssh_private_key_path))
      host        = self.ipv4_address
      timeout     = "5m"
    }
  }

  depends_on = [
    data.digitalocean_ssh_key.default
  ]
}

# DigitalOcean Firewall for security
resource "digitalocean_firewall" "minitwit" {
  count = var.deployment_mode == "production" && var.digitalocean_firewall_enable ? 1 : 0
  name  = "${var.project_name}-${var.environment}-firewall"

  droplet_ids = [digitalocean_droplet.minitwit[0].id]

  # Allow SSH
  inbound_rule {
    protocol         = "tcp"
    port_range       = "22"
    source_addresses = ["0.0.0.0/0", "::/0"]
  }

  # Allow HTTP
  inbound_rule {
    protocol         = "tcp"
    port_range       = "80"
    source_addresses = ["0.0.0.0/0", "::/0"]
  }

  # Allow HTTPS
  inbound_rule {
    protocol         = "tcp"
    port_range       = "443"
    source_addresses = ["0.0.0.0/0", "::/0"]
  }

  # Allow Grafana (for monitoring access)
  inbound_rule {
    protocol         = "tcp"
    port_range       = "3000"
    source_addresses = ["0.0.0.0/0", "::/0"]
  }

  # Allow Prometheus (internal)
  inbound_rule {
    protocol         = "tcp"
    port_range       = "9090"
    source_addresses = ["127.0.0.1/32"]
  }

  # Allow Loki (internal)
  inbound_rule {
    protocol         = "tcp"
    port_range       = "3100"
    source_addresses = ["127.0.0.1/32"]
  }

  # Allow all outbound traffic
  outbound_rule {
    protocol              = "tcp"
    port_range            = "1-65535"
    destination_addresses = ["0.0.0.0/0", "::/0"]
  }

  outbound_rule {
    protocol              = "udp"
    port_range            = "1-65535"
    destination_addresses = ["0.0.0.0/0", "::/0"]
  }

  tags = [var.project_name, var.environment]
}

# DigitalOcean Reserved IP for static access (optional)
resource "digitalocean_reserved_ip" "minitwit" {
  count     = var.deployment_mode == "production" ? 1 : 0
  name      = "${var.project_name}-${var.environment}-reserved-ip"
  region    = var.digitalocean_region
  droplet_id = digitalocean_droplet.minitwit[0].id
}

# ============================================================================
# Ansible Provisioning
# ============================================================================

# Generate Ansible inventory file
resource "local_file" "ansible_inventory" {
  filename = "${path.module}/inventory.ini"

  content = var.deployment_mode == "local" ? templatefile("${path.module}/templates/inventory-local.tpl", {
    domain = var.domain_name
  }) : templatefile("${path.module}/templates/inventory-production.tpl", {
    droplet_ip           = digitalocean_droplet.minitwit[0].ipv4_address
    ssh_key_path        = pathexpand(var.ssh_private_key_path)
    project_name        = var.project_name
  })

  depends_on = [
    digitalocean_droplet.minitwit
  ]
}

# Run Ansible provisioning
resource "null_resource" "ansible_provisioning" {
  count = var.provision_with_ansible ? 1 : 0

  provisioner "local-exec" {
    command = var.deployment_mode == "local" ? 
      "echo 'Skipping Ansible provisioning for local deployment (handled by cloud-init)'" :
      "sleep 30 && ansible-playbook -i ${local_file.ansible_inventory.filename} ${var.ansible_playbook_path} ${var.ansible_verbose ? "-vvv" : ""}"
  }

  depends_on = [
    local_file.ansible_inventory,
    digitalocean_droplet.minitwit
  ]
}

# ============================================================================
# Output Information for Access
# ============================================================================

output "deployment_info" {
  value = var.deployment_mode == "local" ? {
    mode        = "Local Development (VirtualBox)"
    web_app     = "http://localhost:${var.local_host_port_http}"
    grafana     = "http://localhost:${var.local_host_port_grafana}"
    prometheus  = "http://localhost:${var.local_host_port_prometheus}"
    loki        = "http://localhost:${var.local_host_port_loki}"
    message     = "✓ MiniTwit Local infrastructure ready"
  } : {
    mode           = "Production (DigitalOcean)"
    droplet_ip     = digitalocean_droplet.minitwit[0].ipv4_address
    droplet_id     = digitalocean_droplet.minitwit[0].id
    reserved_ip    = try(digitalocean_reserved_ip.minitwit[0].ip_address, "N/A")
    web_app        = "https://${var.domain_name}"
    grafana        = "http://${digitalocean_droplet.minitwit[0].ipv4_address}:3000"
    ssh_command    = "ssh -i ${var.ssh_private_key_path} root@${digitalocean_droplet.minitwit[0].ipv4_address}"
    message        = "✓ MiniTwit Production infrastructure deployed"
  }
  description = "Deployment information and access URLs"
}
