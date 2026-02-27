Vagrant.configure("2") do |config|
  config.vm.box = "ubuntu/focal64"
  config.vm.network "forwarded_port", guest: 80, host: 8080

  # Install Ansible on the VM so it can provision itself
  config.vm.provision "shell", inline: <<-SHELL
    apt-get update
    apt-get install -y python3-pip
    pip3 install ansible
    ansible-galaxy collection install community.docker
  SHELL

  # Run the playbook locally inside the VM
  config.vm.provision "ansible_local" do |ansible|
    ansible.playbook = "ansible/playbook.yml"
    ansible.install = false # We installed it via shell above
  end
end