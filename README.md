# MiniTwit 

## 🛠 Development Environments

We use **Ansible** as our configuration "Source of Truth" to ensure that our local development environment perfectly matches our production server.

### 1. Local Setup (Vagrant + Ansible)
To start a local Ubuntu VM that mimics our production server on your own machine:

1.  **Prerequisites:** Install [Vagrant](https://www.vagrantup.com/) and [VirtualBox](https://www.virtualbox.org/).
2.  **Start the VM:**
    ```bash
    vagrant up
    ```
    *This command automatically creates a VM, installs Docker via Ansible, and starts the MiniTwit container.*
3.  **Access:** Open [http://localhost:8080](http://localhost:8080) in your browser.

**Common Management Commands:**
* `vagrant halt`: Power down the local VM.
* `vagrant provision`: Re-run the Ansible playbook to apply configuration changes to the VM.
* `vagrant destroy`: Completely delete the local VM.

---

### 2. Manual Docker (Quick Start)
If you wish to run the container directly on your host machine without the Vagrant VM:

# Build the image locally
```bash
docker build -t minitwit/webserver ./itu-minitwit
```
# Run with persistent volume mapping
```bash

docker run -d \
  -p 5273:5273 \
  --name minitwit \
  -v minitwit_db_volume:/app/data \
  -e CHIRPDBPATH=/app/data/minitwit.db \
  minitwit/webserver
```
---
### Required GitHub Secrets
To enable successful deployments, the following secrets must be configured in the repository:

* **`SSH_HOST`**: The IP address of the production server.
* **`SSH_USER`**: The deployment user (typically `root`).
* **`SSH_KEY`**: The private SSH key for server access.
* **`DOCKERHUB_USERNAME`**: Your Docker Hub account ID.
* **`DOCKERHUB_TOKEN`**: A Personal Access Token (PAT) for Docker Hub.
