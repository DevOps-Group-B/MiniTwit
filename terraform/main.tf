# ============================================================================
# MiniTwit Infrastructure as Code - Main Configuration
# ============================================================================

# Fetch DigitalOcean SSH key
data "digitalocean_ssh_key" "default" {
  name = var.digitalocean_ssh_key_name
}

# 1. WEB SERVER DROPLET
resource "digitalocean_droplet" "minitwit" {
  name       = "${var.project_name}-${var.environment}-web"
  region     = var.digitalocean_region
  size       = var.digitalocean_droplet_size
  image      = "ubuntu-24-04-x64" # Updated to match your OS screenshot
  ssh_keys   = [data.digitalocean_ssh_key.default.id]
  tags       = [var.project_name, var.environment, "web"]
}

# 2. DATABASE DROPLET
resource "digitalocean_droplet" "database" {
  name       = "${var.project_name}-${var.environment}-db"
  region     = var.digitalocean_region
  size       = "s-1vcpu-1gb"
  image      = "ubuntu-24-04-x64"
  ssh_keys   = [data.digitalocean_ssh_key.default.id]
  tags       = [var.project_name, var.environment, "database"]
}

# 3. FIREWALLS
resource "digitalocean_firewall" "web" {
  name        = "${var.project_name}-web-firewall"
  droplet_ids = [digitalocean_droplet.minitwit.id]

  inbound_rule {
    protocol         = "tcp"
    port_range       = "22"
    source_addresses = ["0.0.0.0/0", "::/0"]
  }

  inbound_rule {
    protocol         = "tcp"
    port_range       = "80"
    source_addresses = ["0.0.0.0/0", "::/0"]
  }

  inbound_rule {
    protocol         = "tcp"
    port_range       = "443"
    source_addresses = ["0.0.0.0/0", "::/0"]
  }

  outbound_rule {
    protocol              = "tcp"
    port_range            = "1-65535"
    destination_addresses = ["0.0.0.0/0", "::/0"]
  }
}

resource "digitalocean_firewall" "database" {
  name        = "${var.project_name}-db-firewall"
  droplet_ids = [digitalocean_droplet.database.id]

  inbound_rule {
    protocol         = "tcp"
    port_range       = "22"
    source_addresses = ["0.0.0.0/0", "::/0"]
  }

  inbound_rule {
    protocol         = "tcp"
    port_range       = "5432"
    source_droplet_ids = [digitalocean_droplet.minitwit.id] # ONLY Web can talk to DB
  }

  outbound_rule {
    protocol              = "tcp"
    port_range            = "1-65535"
    destination_addresses = ["0.0.0.0/0", "::/0"]
  }
}

# 4. ANSIBLE INVENTORY GENERATION
resource "local_file" "ansible_inventory" {
  filename = "${path.module}/../ansible/inventory.ini"
  content  = templatefile("${path.module}/templates/inventory-production.tpl", {
    web_ip       = digitalocean_droplet.minitwit.ipv4_address
    db_ip        = digitalocean_droplet.database.ipv4_address
    ssh_key_path = pathexpand(var.ssh_private_key_path)
    project_name = var.project_name
  })
}

# 5. WAIT AND PROVISION
resource "time_sleep" "wait_for_droplets" {
  create_duration = "30s"
  depends_on      = [digitalocean_droplet.minitwit, digitalocean_droplet.database]
}

resource "null_resource" "ansible_provisioning" {
  count      = var.provision_with_ansible ? 1 : 0
  depends_on = [time_sleep.wait_for_droplets, local_file.ansible_inventory]

  provisioner "local-exec" {
    command = "ansible-playbook -i ${local_file.ansible_inventory.filename} ${var.ansible_playbook_path}"
  }
}