# ============================================================================
# MiniTwit Infrastructure as Code - Main Configuration
# ============================================================================
# This configuration manages production (DigitalOcean) infrastructure only

locals {
  common_tags = merge(
    var.common_tags,
    {
      Project       = var.project_name
      ManagedBy     = "Terraform"
      DeploymentMode = "production"
      CreatedAt     = timestamp()
    }
  )
}

# ============================================================================
# Production Infrastructure (DigitalOcean)
# ============================================================================

# Fetch DigitalOcean SSH key for droplet access
data "digitalocean_ssh_key" "default" {
  name  = var.digitalocean_ssh_key_name
}

# DigitalOcean Droplet (VPS)
resource "digitalocean_droplet" "minitwit" {
  name               = "${var.project_name}-${var.environment}-droplet"
  region             = var.digitalocean_region
  size               = var.digitalocean_droplet_size
  image              = "ubuntu-22-04-x64"
  backups            = true
  ipv6               = true
  monitoring         = true

  ssh_keys = [data.digitalocean_ssh_key.default.id]

  tags = [var.project_name, var.environment]

  # Provision the instance after creation
  provisioner "remote-exec" {
    inline = [
      "while pgrep -x apt-get > /dev/null; do sleep 1; done",
      "sudo cloud-init status --wait",
      "sudo DEBIAN_FRONTEND=noninteractive apt-get update",
      "sudo DEBIAN_FRONTEND=noninteractive apt-get install -y python3-pip python3-venv git curl",
      "pip3 install --upgrade ansible",
      "ansible-galaxy collection install community.docker"
    ]

    connection {
      type        = "ssh"
      user        = "root"
      agent       = true
      host        = self.ipv4_address
      timeout     = "10m"
    }
  }

  depends_on = [
    data.digitalocean_ssh_key.default
  ]
}

# DigitalOcean Firewall for security
resource "digitalocean_firewall" "minitwit" {
  count = var.digitalocean_firewall_enable ? 1 : 0
  name  = "${var.project_name}-${var.environment}-firewall"

  droplet_ids = [digitalocean_droplet.minitwit.id]

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

# ============================================================================
# Ansible Provisioning
# ============================================================================

# Generate Ansible inventory file
resource "local_file" "ansible_inventory" {
  filename = "${path.module}/inventory.ini"

  content = templatefile("${path.module}/templates/inventory-production.tpl", {
    droplet_ip   = digitalocean_droplet.minitwit.ipv4_address
    ssh_key_path = pathexpand(var.ssh_private_key_path)
    project_name = var.project_name
  })

  depends_on = [
    digitalocean_droplet.minitwit
  ]
}

# Run Ansible provisioning
resource "null_resource" "ansible_provisioning" {
  count = var.provision_with_ansible ? 1 : 0

  provisioner "local-exec" {
    command = "sleep 30 && ansible-playbook -i ${local_file.ansible_inventory.filename} ${var.ansible_playbook_path} ${var.ansible_verbose ? "-vvv" : ""}"
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
  value = {
    mode        = "Production (DigitalOcean)"
    droplet_ip  = digitalocean_droplet.minitwit.ipv4_address
    droplet_id  = digitalocean_droplet.minitwit.id
    web_app     = "https://${var.domain_name}"
    grafana     = "http://${digitalocean_droplet.minitwit.ipv4_address}:3000"
    ssh_command = "ssh -i ${var.ssh_private_key_path} root@${digitalocean_droplet.minitwit.ipv4_address}"
    message     = "✓ MiniTwit Production infrastructure deployed"
  }
  description = "Deployment information and access URLs"
}
