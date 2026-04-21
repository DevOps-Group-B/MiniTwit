Vagrant.configure("2") do |config|
  config.vm.box = "ubuntu/jammy64"
  config.vm.boot_timeout = 600
  config.vm.network "forwarded_port", guest: 80, host: 8080
  config.vm.network "forwarded_port", guest: 3000, host: 3000
  config.vm.network "forwarded_port", guest: 9090, host: 9090
  #Loki port
  config.vm.network "forwarded_port", guest: 3100, host: 3100
  config.vm.network "forwarded_port", guest: 12345, host: 12345
  config.ssh.insert_key = false
  config.ssh.keep_alive = true
  config.ssh.connect_timeout = 60

  config.vm.provider "virtualbox" do |vb|
    vb.memory = 4096
    vb.cpus = 2
    # Force a plain BIOS boot for this box. On some recent VirtualBox setups,
    # relying on the provider default can leave the imported guest stuck before
    # SSH becomes reachable.
    vb.customize ["modifyvm", :id, "--firmware", "bios"]
  end

  # Install Ansible on the VM so it can provision itself
  config.vm.provision "shell", inline: <<-SHELL
    apt-get update
    apt-get install -y python3-pip python3-venv
    pip3 install --upgrade ansible
    ansible-galaxy collection install community.docker
  SHELL

  # Run the playbook locally inside the VM
  config.vm.provision "ansible_local" do |ansible|
    ansible.playbook = "ansible/playbook.yml"
    ansible.install = false # We installed it via shell above
    ansible.extra_vars = {
      deployment_mode: "vagrant",
      dockerhub_username: "sn3ke",
      image_tag: "latest",
      postgres_connection_string: "",
      grafana_admin_user: "admin",
      grafana_admin_password: "admin",
      domain_name: "localhost",
      letsencrypt_email: "local@minitwit.test"
    }
  end
end
