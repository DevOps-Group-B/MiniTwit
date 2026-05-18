# MiniTwit 
This is Group B's exam submission for the DevOps, Software Evolution and Software Maintenance BSc course at ITU  

MiniTwit is a .NET 9 application with a web UI, automated tests, and infrastructure automation for local and production deployment. This repository combines the application code, monitoring stack, and deployment configuration used to run the project.

## Description

The repository is structured around three main areas:

- `itu-minitwit/`: the ASP.NET Core application, tests, and monitoring assets
- `ansible/`: deployment automation for server configuration and application rollout
- `terraform/`: infrastructure provisioning for production environments

The application includes:

- simulator endpoints for coursework-style automated interaction
- metrics and logs through Prometheus, Grafana, Loki, and Alloy

## Demo

The current repository documentation references these deployment endpoints:

- Application: `https://minitwit.tech`, `https://www.minitwit.tech/` or `http://165.227.170.149`
- Monitoring UI: `http://165.227.170.149:3000`

They are included as references for the project report and infrastructure setup.
If they change, this file should be updated accordingly.

## Requirements

Choose the toolchain based on how you want to work with the project locally.

### Application development

- .NET 9 SDK
- optional: PostgreSQL 15+ if you do not want to use the SQLite fallback

### Container-based runs

- Docker Engine or Docker Desktop
- Docker Compose

### Infrastructure automation

- Ansible
- Terraform
- optional: Vagrant and VirtualBox for the legacy local VM flow

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

### Production Log Rotation (logrotate)
Production deployments via Ansible now install `logrotate` and deploy a policy in
`/etc/logrotate.d/minitwit-docker-containers` for Docker JSON logs at
`/var/lib/docker/containers/*/*-json.log`.

Policy:

* weekly rotation
* rotate 14 archives
* size threshold 50M
* compress + delaycompress
* dateext
* copytruncate

Quick verification on a server:

```bash
sudo ls -l /etc/logrotate.d/minitwit-docker-containers
sudo cat /etc/logrotate.d/minitwit-docker-containers
sudo logrotate --debug /etc/logrotate.conf
```

Force a test rotation:

```bash
sudo logrotate --force /etc/logrotate.conf
sudo ls -lah /var/lib/docker/containers/*/*-json.log*
docker logs --tail 20 minitwit
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

