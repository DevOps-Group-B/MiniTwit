# MiniTwit (Chirp!) 🐦

> A Twitter-like social media platform built with ASP.NET 9.0, deployed on DigitalOcean with full DevOps automation.

MiniTwit (nicknamed **Chirp!**) is a Group B exam project for the *DevOps, Software Evolution and Software Maintenance* BSc course at IT University of Copenhagen. It is a microblogging application where users can post short messages ("cheeps"), follow other users, and earn achievements — all backed by a production-grade DevOps pipeline.

**🌐 Live application:** https://minitwit.tech  
**📊 Monitoring (Grafana):** http://209.38.113.150:3000/dashboards


## Getting Started

### Prerequisites

**For local development:**
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- Docker Engine or Docker Desktop (for the Compose option)

**For infrastructure automation:**
- Ansible
- Terraform / OpenTofu
- A DigitalOcean account + API token

---

### Option 1: Run locally with .NET

The simplest path — uses SQLite automatically when no PostgreSQL connection string is set.

```bash
cd itu-minitwit
dotnet restore
dotnet build
dotnet run --project src/MiniTwit.Web
```

App available at: **http://localhost:5273**

---

### Option 2: Run with Docker Compose

Starts the full stack — application, PostgreSQL, Prometheus, Grafana, Loki, and Alloy.

```bash
cp .env.example .env
# Edit .env and fill in POSTGRES_CONNECTION_STRING, GRAFANA_ADMIN_USER, GRAFANA_ADMIN_PASSWORD
docker compose up --build
```

| Service | URL |
|---------|-----|
| Application | http://localhost:8080 |
| Grafana | http://localhost:3000 |
| Prometheus | http://localhost:9090 |
| Loki | http://localhost:3100 |

---

### Option 3: Provision infrastructure with Terraform

Provisions the full DigitalOcean infrastructure (droplets, firewalls, floating IP).

```bash
cd terraform
terraform init
cp terraform.tfvars.example terraform.tfvars
# Fill in terraform.tfvars with your DO token, SSH key, domain, etc.
terraform plan
terraform apply
```


---

## Running Tests

The test suite includes unit, integration, and end-to-end (Playwright) tests.

```bash
cd itu-minitwit

# Restore and build first
dotnet restore
dotnet build

# Install Playwright browsers (needed for E2E tests)
pwsh tests/End2end/bin/Debug/net9.0/playwright.ps1 install chromium

# Run all tests
dotnet test
```

All tests must pass before any deployment — a failing test blocks the CI/CD pipeline.

---

## Deploying to Production

Production deployments are fully automated via **GitHub Actions + Ansible**. A push to `main` triggers the full pipeline. You can also deploy manually:

### Prerequisites

1. Servers provisioned via Terraform (see above)
2. GitHub Secrets configured (see below)
3. SSH access to the DigitalOcean droplets

### Infrastructure Provisioning (Terraform)

If starting from scratch:

```bash
cd terraform
terraform init
terraform apply
```

Terraform will output the droplet IPs and auto-generate `ansible/inventory.ini`.

### Application Deployment (Ansible)

To deploy manually without GitHub Actions:

```bash
cd ansible
ansible-playbook playbook.yml \
  -i inventory.ini \
  --extra-vars "image_tag=latest \
    dockerhub_username=<your_dockerhub_user> \
    dockerhub_token=<your_token> \
    postgres_connection_string='Host=<db_ip>;Database=minitwit;Username=minitwit_user;Password=<pw>;Port=5432' \
    grafana_admin_password=<password> \
    do_api_token=<token> \
    keepalived_auth_pass=<vrrp_password> \
    keepalived_virtual_ip=<floating_ip>"
```

### Required GitHub Secrets

Configure these in **Settings → Secrets and variables → Actions** in your GitHub repository:

| Secret | Description |
|--------|-------------|
| `SSH_HOST` | IP of the primary LB droplet |
| `SSH_HOST_SECONDARY` | IP of the secondary LB droplet |
| `SSH_USER` | SSH user (typically `root`) |
| `SSH_KEY` | Private SSH key for server access |
| `DOCKERHUB_USERNAME` | Docker Hub account ID |
| `DOCKERHUB_TOKEN` | Docker Hub Personal Access Token |
| `POSTGRES_CONNECTION_STRING` | Full PostgreSQL connection string |
| `GRAFANA_ADMIN_USER` | Grafana admin username |
| `GRAFANA_ADMIN_PASSWORD` | Grafana admin password |
| `LETSENCRYPT_EMAIL` | Email for Let's Encrypt certificate registration |
| `DO_API_TOKEN` | DigitalOcean API token (for floating IP failover) |
| `KEEPALIVED_AUTH_PASS` | Keepalived VRRP authentication password |
| `FLOATING_IP` | DigitalOcean floating IP for HA failover |

---


## Contributing

We follow a trunk-based development model — all changes go to `main` via pull requests.

1. Fork the repository and create a feature branch:
   ```bash
   git checkout -b feature/my-feature
   ```
2. Make your changes. Ensure code is formatted:
   ```bash
   dotnet format
   ```
3. Run the full test suite locally:
   ```bash
   dotnet test
   ```
4. Open a pull request against `main`. The CI pipeline runs automatically on PR updates.
5. Once approved and merged to `main`, the CD pipeline deploys to production automatically.

> **Note**: The pipeline will reject any PR that fails formatting checks, static analysis, or tests. Fix these locally before pushing.

---

## Monitoring & Observability

The full observability stack is deployed alongside the application:

| Tool | Purpose | URL (production) |
|------|---------|-----------------|
| **Grafana** | Dashboards & alerting | http://minitwit.tech:3000 |
| **Prometheus** | Metrics collection | http://minitwit.tech:9090 |
| **Loki** | Log aggregation | (via Grafana Explore) |
| **Alloy** | Log collection from Docker | internal |
| **Node Exporter** | System metrics (CPU, memory, disk) | internal |

Grafana dashboards are provisioned as code from `monitoring/grafana/dashboards/`


---

