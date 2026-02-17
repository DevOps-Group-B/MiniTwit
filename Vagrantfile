# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  config.vm.box = "digital_ocean"
  config.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"


  config.vm.provider :digital_ocean do |provider, override|
    # Ensure you have exported ENV tokens PC specific details in your shell before running vagrant up
    provider.token = ENV["DIGITAL_OCEAN_TOKEN"]
    provider.ssh_key_name = ENV["SSH_KEY_NAME"]
    # Path to your private SSH key
    override.ssh.private_key_path = ENV['SSH_KEY_PATH']
    # VM details
    provider.image = 'ubuntu-22-04-x64'
    provider.region = 'fra1'
    provider.size = 's-1vcpu-1gb'
    provider.name = 'minitwit-server'
  end

  # Disable default folder sync
  config.vm.synced_folder ".", "/vagrant", disabled: true

  config.vm.provision "shell", inline: <<-SHELL
    export DEBIAN_FRONTEND=noninteractive

    # --- 1. Install Docker & Git ---
    apt-get update
    apt-get install -y git apt-transport-https ca-certificates curl software-properties-common

    # Add Docker's official GPG key
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg

    # Set up the stable repository
    echo \
      "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu \
      $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

    apt-get update
    apt-get install -y docker-ce docker-ce-cli containerd.io

    # --- 2. Clone Repository ---
    # Remove previous folder if re-provisioning to force a fresh clone
    rm -rf /root/MiniTwit

    echo "Cloning repository..."
    git clone https://github.com/DevOps-Group-B/MiniTwit.git /root/MiniTwit

    # --- 3. Build & Run Application ---
    # Navigate to the specific subfolder containing the Dockerfile
    cd /root/MiniTwit/itu-minitwit

    echo "Building Docker Image..."
    docker build -t minitwit/webserver .

    echo "Cleaning up old containers..."
    # '|| true' prevents the script from failing if the container doesn't exist yet
    docker stop minitwit || true
    docker rm minitwit || true

    echo "Starting new container..."
    # Map port 80 on the VM to port 5273 inside the container
    docker run -d --restart always -p 80:5273 --name minitwit minitwit/webserver

    echo "================================================================="
    echo "=                            DONE                               ="
    echo "================================================================="
    THIS_IP=$(curl -s ifconfig.me)
    echo "Navigate in your browser to: http://${THIS_IP}"
  SHELL
end