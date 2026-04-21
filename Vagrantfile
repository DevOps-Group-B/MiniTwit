Vagrant.configure("2") do |config|
  do_plugin_installed = Vagrant.has_plugin?("vagrant-digitalocean")
  do_enabled = do_plugin_installed && 
               ENV["DIGITALOCEAN_TOKEN"] &&
               ENV["DIGITALOCEAN_SSH_KEY_IDS"]

  configure_common = lambda do |machine_config, extra_vars|
    machine_config.ssh.insert_key = false
    machine_config.ssh.keep_alive = true
    machine_config.ssh.connect_timeout = 60
    # Install Ansible on the guest so it can provision itself.
    machine_config.vm.provision "shell", inline: <<-SHELL
      apt-get update
      apt-get install -y python3-pip python3-venv
      pip3 install --upgrade ansible
      ansible-galaxy collection install community.docker
    SHELL

    machine_config.vm.provision "ansible_local" do |ansible|
      ansible.playbook = "ansible/playbook.yml"
      ansible.install = false
      ansible.extra_vars = {
        deployment_mode: "vagrant",
        dockerhub_username: ENV.fetch("DOCKERHUB_USERNAME", "sn3ke"),
        image_tag: ENV.fetch("IMAGE_TAG", "latest"),
        postgres_connection_string: ENV.fetch("POSTGRES_CONNECTION_STRING", ""),
        grafana_admin_user: ENV.fetch("GRAFANA_ADMIN_USER", "admin"),
        grafana_admin_password: ENV.fetch("GRAFANA_ADMIN_PASSWORD", "admin")
      }.merge(extra_vars)
    end
  end

  config.vm.define "default" do |local|
    local.vm.box = "ubuntu/jammy64"
    local.vm.boot_timeout = 600

    # App - use port 5273 inside the service, forwarded through guest port 80.
    local.vm.network "forwarded_port", guest: 80, host: 8080
    local.vm.network "forwarded_port", guest: 3000, host: 3000
    local.vm.network "forwarded_port", guest: 9090, host: 9090
    local.vm.network "forwarded_port", guest: 3100, host: 3100
    local.vm.network "forwarded_port", guest: 12345, host: 12345

    local.vm.provider "virtualbox" do |vb|
      vb.memory = 4096
      vb.cpus = 2
      # Force a plain BIOS boot for this box. On some recent VirtualBox setups,
      # relying on the provider default can leave the imported guest stuck before
      # SSH becomes reachable.
      vb.customize ["modifyvm", :id, "--firmware", "bios"]
    end

    configure_common.call(local, {
      domain_name: "localhost",
      letsencrypt_email: "local@minitwit.test"
    })
  end

  if do_enabled
    config.vm.define "digital_ocean" do |cloud|
      cloud.vm.box = "digital_ocean"
      cloud.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"
      cloud.vm.boot_timeout = 600
      cloud.vm.hostname = ENV.fetch("DIGITALOCEAN_DROPLET_NAME", "minitwit-vagrant")

      # DigitalOcean guests need a remote sync implementation for /vagrant.
      cloud.vm.synced_folder ".", "/vagrant",
        type: "rsync",
        rsync__exclude: [".git/", ".vagrant/"]

      cloud.ssh.username = ENV.fetch("DIGITALOCEAN_SSH_USER", "root")
      cloud.ssh.private_key_path = [
        ENV.fetch("DIGITALOCEAN_SSH_PRIVATE_KEY_PATH", File.expand_path("~/.ssh/id_rsa"))
      ]

      cloud.vm.provider "digital_ocean" do |provider|
        provider.token = ENV["DIGITALOCEAN_TOKEN"]
        provider.image = ENV.fetch("DIGITALOCEAN_IMAGE", "ubuntu-22-04-x64")
        provider.region = ENV.fetch("DIGITALOCEAN_REGION", "fra1")
        provider.size = ENV.fetch("DIGITALOCEAN_SIZE", "s-2vcpu-4gb")
        provider.ssh_key_ids = ENV["DIGITALOCEAN_SSH_KEY_IDS"].split(",").map(&:strip)
        provider.private_networking = true if provider.respond_to?(:private_networking=)
        provider.ipv6 = true if provider.respond_to?(:ipv6=)
        provider.monitoring = true if provider.respond_to?(:monitoring=)
      end

      configure_common.call(cloud, {
        domain_name: ENV.fetch("DOMAIN_NAME", "minitwit.tech"),
        letsencrypt_email: ENV.fetch("LETSENCRYPT_EMAIL", "local@minitwit.test")
      })
    end
  elsif do_plugin_installed
    warn "DigitalOcean provider detected, but DIGITALOCEAN_TOKEN and/or DIGITALOCEAN_SSH_KEY_IDS are missing. Skipping DigitalOcean machine."
  else
    warn "vagrant-digitalocean is not installed. Skipping DigitalOcean machine."
  end
end
