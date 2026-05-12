# MiniTwit Vagrantfile
# This file defines Infrastructure as Code for local development (VirtualBox) 
# and production deployment (DigitalOcean)
#
# Local Usage:
#   vagrant up                    # Spin up local VM
#   vagrant provision             # Re-run Ansible playbook
#   vagrant halt                  # Power down VM
#   vagrant destroy               # Delete VM
#
# DigitalOcean Usage (requires vagrant-digitalocean plugin and env vars):
#   vagrant plugin install vagrant-digitalocean
#   export DIGITALOCEAN_TOKEN=your_token
#   export DIGITALOCEAN_SSH_KEY_IDS=key_id
#   vagrant up --provider=digitalocean

Vagrant.configure("2") do |config|
  # ============================================================================
  # DIGITALOCEAN PROVIDER DETECTION
  # ============================================================================
  do_plugin_installed = Vagrant.has_plugin?("vagrant-digitalocean")
  do_enabled = do_plugin_installed &&
               ENV["DIGITALOCEAN_TOKEN"] &&
               ENV["DIGITALOCEAN_SSH_KEY_IDS"]

  if do_enabled
    puts "✓ DigitalOcean provider available"
  end

  # ============================================================================
  # COMMON CONFIGURATION LAMBDA (applies to all VMs)
  # ============================================================================
  configure_common = lambda do |machine_config, extra_vars|
    # SSH Configuration
    machine_config.ssh.insert_key = false
    machine_config.ssh.keep_alive = true
    machine_config.ssh.connect_timeout = 60

    # Install Ansible on the guest for local provisioning
    machine_config.vm.provision "shell", inline: <<-SHELL
      set -e
      echo "Installing Python and Ansible..."
      apt-get update
      apt-get install -y python3-pip python3-venv git curl
      pip3 install --upgrade ansible
      ansible-galaxy collection install community.docker
      echo "✓ Ansible installed successfully"
    SHELL

    # Run Ansible playbook to provision the VM
    machine_config.vm.provision "ansible_local" do |ansible|
      ansible.playbook = "ansible/playbook.yml"
      ansible.install = false
      ansible.verbose = false
      ansible.extra_vars = {
        deployment_mode: "vagrant",
        dockerhub_username: ENV.fetch("DOCKERHUB_USERNAME", "sn3ke"),
        image_tag: ENV.fetch("IMAGE_TAG", "latest"),
        postgres_connection_string: ENV.fetch("POSTGRES_CONNECTION_STRING", ""),
        grafana_admin_user: ENV.fetch("GRAFANA_ADMIN_USER", "admin"),
        grafana_admin_password: ENV.fetch("GRAFANA_ADMIN_PASSWORD", "admin"),
        domain_name: ENV.fetch("DOMAIN_NAME", "localhost"),
        letsencrypt_email: ENV.fetch("LETSENCRYPT_EMAIL", "local@minitwit.test")
      }.merge(extra_vars)
    end
  end

  # ============================================================================
  # LOCAL DEVELOPMENT VM (VirtualBox Provider)
  # ============================================================================
  config.vm.define "default" do |local|
    local.vm.box = "ubuntu/jammy64"
    local.vm.boot_timeout = 600

    # Port Forwarding (maps guest ports to host machine)
    # Port 80 (HTTP) → 8080 (local web app via nginx-proxy)
    # Port 3000 (Grafana dashboards)
    # Port 9090 (Prometheus metrics)
    # Port 3100 (Loki logs)
    # Port 12345 (Debugging/auxiliary)
    local.vm.network "forwarded_port", guest: 80, host: 8080, auto_correct: true
    local.vm.network "forwarded_port", guest: 443, host: 8443, auto_correct: true
    local.vm.network "forwarded_port", guest: 3000, host: 3000, auto_correct: true
    local.vm.network "forwarded_port", guest: 9090, host: 9090, auto_correct: true
    local.vm.network "forwarded_port", guest: 3100, host: 3100, auto_correct: true
    local.vm.network "forwarded_port", guest: 12345, host: 12345, auto_correct: true

    # Private network for internal VM communication
    local.vm.network "private_network", type: "dhcp"

    # VirtualBox Provider Configuration
    local.vm.provider "virtualbox" do |vb|
      vb.name = "MiniTwit-Local"
      vb.memory = 4096      # 4 GB RAM
      vb.cpus = 2           # 2 CPU cores
      
      # Network optimization
      vb.customize ["modifyvm", :id, "--nictype1", "virtio"]
      vb.customize ["modifyvm", :id, "--nictype2", "virtio"]
      
      # Force BIOS boot to ensure SSH becomes reachable
      vb.customize ["modifyvm", :id, "--firmware", "bios"]
      
      # Enable clipboard and drag-drop (optional)
      vb.customize ["modifyvm", :id, "--clipboard", "bidirectional"]
      vb.customize ["modifyvm", :id, "--draganddrop", "bidirectional"]
      
      # Disable unnecessary USB
      vb.customize ["modifyvm", :id, "--usb", "off"]
      vb.customize ["modifyvm", :id, "--usbehci", "off"]
    end

    # Apply common configuration
    configure_common.call(local, {})

    # Post-provisioning message
    local.vm.post_up_message = "✓ MiniTwit Local VM is up!\n" \
                               "  Web App: http://localhost:8080\n" \
                               "  Grafana: http://localhost:3000\n" \
                               "  Prometheus: http://localhost:9090\n" \
                               "  Loki: http://localhost:3100"
  end

  # ============================================================================
  # PRODUCTION VM (DigitalOcean Provider) - Optional
  # ============================================================================
  config.vm.define "production", autostart: false do |production|
    production.vm.box = "digital_ocean"
    production.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"
    production.ssh.private_key_path = ENV.fetch("SSH_PRIVATE_KEY_PATH", "~/.ssh/id_rsa")

    production.vm.provider :digitalocean do |provider, override|
      provider.token = ENV.fetch("DIGITALOCEAN_TOKEN", "")
      provider.image = "ubuntu-22-04-x64"
      provider.region = ENV.fetch("DIGITALOCEAN_REGION", "sfo3")
      provider.size = ENV.fetch("DIGITALOCEAN_SIZE", "s-1vcpu-2gb")
      provider.ssh_key_name = ENV.fetch("DIGITALOCEAN_SSH_KEY_NAME", "MiniTwit")
      
      override.ssh.username = "root"
      override.ssh.private_key_path = ENV.fetch("SSH_PRIVATE_KEY_PATH", "~/.ssh/id_rsa")
    end

    # Apply common configuration with production-specific vars
    production.vm.provision "shell", inline: "echo 'Provisioning DigitalOcean server...'"
    configure_common.call(production, {
      deployment_mode: "production"
    })

    production.vm.post_up_message = "✓ MiniTwit Production VM deployed on DigitalOcean!"
  end

  # ============================================================================
  # GLOBAL SETTINGS
  # ============================================================================
  config.vm.synced_folder ".", "/vagrant", type: "virtualbox"
  
  # Disable default synced folder if using NFS or rsync
  # config.vm.synced_folder ".", "/vagrant", disabled: true

  # Increase timeout for slower connections
  config.vm.boot_timeout = 600
  config.ssh.connect_timeout = 60
end
