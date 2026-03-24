# MiniTwit 
Our links:
Minitwit Website: http://165.227.170.149
Monitoring: http://165.227.170.149:3000

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

### 2. Docker Compose (For local testing)
Run both the application and PostgreSQL database together:

```bash
# Create .env file with database credentials
cp .env.example .env

# Start all services
docker compose up

# Access the application
open http://localhost
```

**Stop services:**
```bash
docker-compose down
```
> ⚠️ **Note:** This connects directly to the **production database**. Any cheeps or users you create will be real.

### Logging Stack (Loki + Alloy)
The Docker Compose setup includes a Grafana Loki logging stack with Grafana Alloy:

* `alloy` tails Docker container logs from `/var/lib/docker/containers/*/*-json.log`
* Alloy parses JSON log records and ships them to `loki`
* Grafana auto-provisions Loki as a datasource (`uid: loki`)

After `docker compose up`, open Grafana Explore and query logs, for example:

```logql
{job="docker"}
```

To focus on HTTP request logs from the web app, try:

```logql
{job="docker"} |= "HTTP"
```

---

### 3. Manual Docker (Single Container)
If you wish to run only the web container without PostgreSQL:

```bash
# Build the image locally
docker build -t minitwit/webserver ./itu-minitwit

# Run with environment variable for PostgreSQL
docker run -d \
  -p 5273:5273 \
  --name minitwit \
  -e POSTGRES_CONNECTION_STRING="Host=your-db-host;Database=minitwit;Username=minitwit_user;Password=your_password;Port=5432" \
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
* **`POSTGRES_CONNECTION_STRING`**: PostgreSQL connection string (e.g., `Host=164.92.164.171;Database=minitwit;Username=minitwit_user;Password=your_password;Port=5432`)

### Database Setup
The application requires a PostgreSQL 15+ database server. Connection details are passed via the `POSTGRES_CONNECTION_STRING` environment variable.

