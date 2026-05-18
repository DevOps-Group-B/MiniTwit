# This looks up your existing SSH key by the name defined in variables.tf
data "digitalocean_ssh_key" "default" {
  name = var.digitalocean_ssh_key_name
}

# ==========================================================
# 1. DROPLETS (HA APP/LB + DB)
# ==========================================================
resource "digitalocean_droplet" "minitwit_lb_primary" {
  name     = "${var.project_name}-lb-primary-prod"
  region   = var.digitalocean_region
  size     = var.digitalocean_droplet_size
  image    = "ubuntu-24-04-x64"
  ssh_keys = [data.digitalocean_ssh_key.default.id]
}

resource "digitalocean_droplet" "minitwit_lb_secondary" {
  name     = "${var.project_name}-lb-secondary-prod"
  region   = var.digitalocean_region
  size     = var.digitalocean_droplet_size
  image    = "ubuntu-24-04-x64"
  ssh_keys = [data.digitalocean_ssh_key.default.id]
}

resource "digitalocean_droplet" "database" {
  name     = "${var.project_name}-db-prod"
  region   = var.digitalocean_region
  size     = var.digitalocean_db_droplet_size
  image    = "ubuntu-24-04-x64"
  ssh_keys = [data.digitalocean_ssh_key.default.id]
}

# ==========================================================
# 2. FLOATING IP (FAILOVER ENDPOINT)
# ==========================================================
resource "digitalocean_floating_ip" "minitwit" {
  region = var.digitalocean_region
}

resource "digitalocean_floating_ip_assignment" "minitwit_primary" {
  ip_address = digitalocean_floating_ip.minitwit.ip_address
  droplet_id = digitalocean_droplet.minitwit_lb_primary.id
}

# ==========================================================
# 3. FIREWALLS
# ==========================================================
resource "digitalocean_firewall" "web" {
  name = "minitwit-web-firewall"
  droplet_ids = [
    digitalocean_droplet.minitwit_lb_primary.id,
    digitalocean_droplet.minitwit_lb_secondary.id,
  ]

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
  name        = "minitwit-db-firewall"
  droplet_ids = [digitalocean_droplet.database.id]

  inbound_rule {
    protocol         = "tcp"
    port_range       = "22"
    source_addresses = ["0.0.0.0/0", "::/0"]
  }

  inbound_rule {
    protocol   = "tcp"
    port_range = "5432"
    source_droplet_ids = [
      digitalocean_droplet.minitwit_lb_primary.id,
      digitalocean_droplet.minitwit_lb_secondary.id,
    ]
  }

  outbound_rule {
    protocol              = "tcp"
    port_range            = "1-65535"
    destination_addresses = ["0.0.0.0/0", "::/0"]
  }
}

# ==========================================================
# 4. ANSIBLE INVENTORY & PROVISIONING
# ==========================================================
resource "local_file" "ansible_inventory" {
  filename = "${path.module}/../ansible/inventory.ini"
  content = templatefile("${path.module}/templates/inventory-production.tpl", {
    primary_web_ip   = digitalocean_droplet.minitwit_lb_primary.ipv4_address
    secondary_web_ip = digitalocean_droplet.minitwit_lb_secondary.ipv4_address
    db_ip            = digitalocean_droplet.database.ipv4_address
    floating_ip      = digitalocean_floating_ip.minitwit.ip_address
    ssh_key_path     = pathexpand(var.ssh_private_key_path)
    project_name     = var.project_name
  })
}

resource "time_sleep" "wait_for_droplets" {
  create_duration = "60s"
  depends_on = [
    digitalocean_droplet.minitwit_lb_primary,
    digitalocean_droplet.minitwit_lb_secondary,
    digitalocean_droplet.database,
  ]
}

resource "null_resource" "ansible_provisioning" {
  count      = var.provision_with_ansible ? 1 : 0
  depends_on = [time_sleep.wait_for_droplets, local_file.ansible_inventory]

  provisioner "local-exec" {
    command = "ansible-playbook -i ${local_file.ansible_inventory.filename} ${var.ansible_playbook_path}"
  }
}
