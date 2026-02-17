Vagrant.configure("2") do |config|
    config.vm.box = "digital_ocean"

    config.vm.provider :digital_ocean do |provider, override|
        # Ensure you have exported DO_TOKEN in your shell before running vagrant up
        provider.token = ENV['DO_TOKEN']

        # Path to your private SSH key
        override.ssh.private_key_path = '~/.ssh/id_rsa'
        override.vm.box = 'digital_ocean'

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

      echo "Deployment complete! Application is running on port 80."
      SHELL
end
# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  # Box setup for DigitalOcean
  config.vm.box = 'digital_ocean'
  config.vm.box_url = "https://github.com/devopsgroup-io/vagrant-digitalocean/raw/master/box/digital_ocean.box"
  
  # Ensure you have your SSH key generated at this path or update it
  config.ssh.private_key_path = '~/.ssh/id_rsa'
  
  # Sync the current folder to /vagrant on the VM
  config.vm.synced_folder ".", "/vagrant", type: "rsync",
    rsync__exclude: [".git/", "itu-minitwit/src/MiniTwit.Web/bin/", "itu-minitwit/src/MiniTwit.Web/obj/"]

  config.vm.define "webserver" do |server|
    server.vm.provider :digital_ocean do |provider|
      # Remember to set these environment variables on your host machine
      provider.token = ENV["DIGITAL_OCEAN_TOKEN"]
      provider.ssh_key_name = ENV["SSH_KEY_NAME"]
      
      provider.image = 'ubuntu-22-04-x64'
      provider.region = 'fra1'
      provider.size = 's-2vcpu-2gb' # Increased size slightly for building .NET
    end

    server.vm.hostname = "minitwit-web"

    server.vm.provision "shell", inline: <<-SHELL
      # 1. Fix potential lock file issues
      sudo fuser -vk -TERM /var/lib/apt/lists/lock
      
      # 2. Install .NET 8.0 SDK (Prerequisites)
      echo "Installing dependencies..."
      wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
      sudo dpkg -i packages-microsoft-prod.deb
      rm packages-microsoft-prod.deb
      
      sudo apt-get update
      sudo apt-get install -y dotnet-sdk-8.0

      # 3. Publish the application
      echo "Building MiniTwit..."
      cd /vagrant/itu-minitwit
      dotnet publish src/MiniTwit.Web/MiniTwit.Web.csproj -c Release -o /home/vagrant/publish

      # 4. Copy database if it exists (Optional, SQLite is a file)
      # If you have a seeded DB you want to preserve, copy it here.
      # Otherwise, the app will create a new one in the temp folder as per Program.cs

      # 5. Run the application
      echo "Starting MiniTwit..."
      
      # Set the DB path to a persistent location relative to our publish folder
      export CHIRPDBPATH="/home/vagrant/publish/minitwit.db"
      
      # Setting URL to listen on all interfaces
      export ASPNETCORE_URLS="http://0.0.0.0:80"

      cd /home/vagrant/publish
      
      # Run in background with nohup
      # We use 'sudo' to bind to port 80
      nohup sudo -E dotnet MiniTwit.Web.dll > /home/vagrant/minitwit.log 2>&1 &

      echo "================================================================="
      echo "=                            DONE                               ="
      echo "================================================================="
      THIS_IP=$(curl -s ifconfig.me)
      echo "Navigate in your browser to: http://${THIS_IP}"
    SHELL
  end
end