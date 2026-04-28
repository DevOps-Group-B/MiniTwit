# ============================================================================
# Local Development Infrastructure (VirtualBox with Libvirt)
# ============================================================================
# This configuration manages the local VirtualBox VM setup for development

# Volume for the VM diskMake sure that creation of your current and complete infrastructure is automated and up-to-date. You decide how to do it. You might want to rely on the tools that we discussed in Session 03

    Using Vagrant (can do most of the things that are required)
    Using shell scripts that use either a) the web-API of the VM provider b) the command line tool of the provider, e.g., doctl for DigitalOcean
    Using a wrapper of the provider's web-API, in the programming language of your choice, e.g., DigitalOcean's official Ruby API wrapper, a Python DigitalOcean API wrapper, etc.

In essence, this task is nothing new. You encoded your infrastructure as code already in session 03. The purpose of this task is to make sure that your IaC is up to date and can (re-)create your actual infrastructure.
Optional: Encode Infrastructure as Code with OpenTofu/Terraform

In case you have energy and resources left, refactor your IaC code to HCL, i.e., so that you use a tool like OpenTofu/Terraform. Do not worry in case you do not have energy and resources for this task. Having a solution described above is good too.
resource "libvirt_volume" "ubuntu" {
  count           = var.deployment_mode == "local" ? 1 : 0
  name            = "${var.local_vm_name}-disk"
  pool            = "default"
  source          = "https://cloud-images.ubuntu.com/jammy/current/jammy-server-cloudimg-amd64.img"
  format          = "qcow2"
  size            = 50 * 1024 * 1024 * 1024  # 50 GB

  lifecycle {
    ignore_changes = [source]
  }
}

# Cloud-init configuration for initial setup
resource "libvirt_cloudinit_disk" "commoninit" {
  count     = var.deployment_mode == "local" ? 1 : 0
  name      = "${var.local_vm_name}-cloud-init"
  pool      = "default"
  user_data = templatefile("${path.module}/templates/cloud-init-local.yml", {
    dockerhub_username = var.dockerhub_username
    image_tag          = var.image_tag
    domain_name        = var.domain_name
    grafana_admin_user = var.grafana_admin_user
    grafana_password   = var.grafana_admin_password
  })
  network_config = templatefile("${path.module}/templates/network-config.yml", {})

  depends_on = [libvirt_volume.ubuntu]
}

# Virtual machine definition
resource "libvirt_domain" "minitwit_local" {
  count   = var.deployment_mode == "local" ? 1 : 0
  name    = var.local_vm_name
  memory  = var.local_vm_memory
  vcpu    = var.local_vm_cpus
  running = true

  # Boot configuration
  boot {
    dev = ["hd"]
  }

  # Network configuration
  network_interface {
    network_name   = "default"
    wait_for_lease = true
  }

  # Disk configuration
  disk {
    volume_id = libvirt_volume.ubuntu[0].id
  }

  # Cloud-init configuration
  cloudinit = libvirt_cloudinit_disk.commoninit[0].id

  # Console configuration
  console {
    type        = "pty"
    target_port = "0"
    target_type = "serial"
  }

  # QEMU/KVM specific settings
  qemu_agent = true
  autostart  = true

  # Provisioners for setup after VM is created
  provisioner "remote-exec" {
    inline = [
      "echo 'Waiting for cloud-init to complete...'",
      "cloud-init status --wait",
      "echo '✓ Cloud-init completed successfully'"
    ]

    connection {
      type        = "ssh"
      user        = "ubuntu"
      private_key = file(pathexpand(var.ssh_private_key_path))
      host        = self.network_interface[0].addresses[0]
      timeout     = "5m"
    }
  }

  depends_on = [
    libvirt_cloudinit_disk.commoninit,
    libvirt_volume.ubuntu
  ]
}

# ============================================================================
# Local Access Configuration
# ============================================================================

# Generate SSH config for easy local access
resource "local_file" "ssh_config_local" {
  count    = var.deployment_mode == "local" ? 1 : 0
  filename = "${path.module}/ssh-config-local"

  content = <<-EOT
Host minitwit-local
    HostName ${try(libvirt_domain.minitwit_local[0].network_interface[0].addresses[0], "127.0.0.1")}
    User ubuntu
    IdentityFile ${pathexpand(var.ssh_private_key_path)}
    StrictHostKeyChecking no
    UserKnownHostsFile /dev/null
EOT
}

# ============================================================================
# Local Port Forwarding Information
# ============================================================================

output "local_vm_info" {
  value = var.deployment_mode == "local" ? {
    vm_name              = var.local_vm_name
    cpus                 = var.local_vm_cpus
    memory_mb            = var.local_vm_memory
    ip_address           = try(libvirt_domain.minitwit_local[0].network_interface[0].addresses[0], "pending")
    access_command       = "ssh ubuntu@${try(libvirt_domain.minitwit_local[0].network_interface[0].addresses[0], 'localhost')}"
    web_app_url          = "http://localhost:${var.local_host_port_http}"
    grafana_url          = "http://localhost:${var.local_host_port_grafana}"
    prometheus_url       = "http://localhost:${var.local_host_port_prometheus}"
    loki_url             = "http://localhost:${var.local_host_port_loki}"
    ssh_config_file      = local_file.ssh_config_local[0].filename
  } : null
  description = "Local VM connection information"
}
