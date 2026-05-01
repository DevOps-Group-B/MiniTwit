# MiniTwit Infrastructure as Code (Terraform)

This directory contains Terraform configurations to manage the distributed MiniTwit infrastructure on DigitalOcean, featuring isolated Web and Database tiers.

## Architecture Overview

The production environment is split across two dedicated nodes:

- **Web Node** — Runs the C# MiniTwit application in Docker, alongside Nginx-Proxy, ACME Companion (SSL), and the monitoring stack (Prometheus, Grafana, Loki, Alloy).
- **Database Node** — A hardened, bare-metal PostgreSQL 16 server.
- **Security** — A DigitalOcean Firewall acts as a DMZ, allowing only the Web Node to reach the Database Node on port 5432.

## Directory Structure

```
terraform/
├── providers.tf                  # Provider configuration (DigitalOcean)
├── variables.tf                  # Input variables (secrets, region, sizes)
├── main.tf                       # Infrastructure definitions (droplets & firewalls)
├── outputs.tf                    # Deployment summaries and SSH strings
├── terraform.tfvars.example      # Template for local variables
└── templates/
    └── inventory-production.tpl  # Template for the multi-node Ansible inventory
```

## Prerequisites

- [Terraform](https://developer.hashicorp.com/terraform/install) >= 1.0
- DigitalOcean API Token (Read/Write)
- SSH key registered in your DigitalOcean account
- GitHub Secrets configured for CI/CD:
  - `SSH_HOST_WEB` — IP of the Web Droplet
  - `SSH_HOST_DB` — IP of the Database Droplet
  - `POSTGRES_CONNECTION_STRING` — `Host=<DB_IP>;Database=minitwit;Username=minitwit_user;Password=<PWD>`

## Quick Start

```bash
# 1. Initialize providers
terraform init

# 2. Create your variables file
cp terraform.tfvars.example terraform.tfvars

# 3. Edit terraform.tfvars with your SSH key name and preferences

# 4. Preview the infrastructure (expect 2 droplets + 2 firewalls)
terraform plan

# 5. Apply
terraform apply

# 6. Verify
terraform output deployment_summary
```

## Configuration

| Variable | Default | Description |
|---|---|---|
| `project_name` | `minitwit` | Prefix for all resource names |
| `digitalocean_region` | `fra1` | Data center location |
| `digitalocean_droplet_size` | `s-1vcpu-2gb` | Size for the Web node |
| `ssh_private_key_path` | `~/.ssh/id_rsa` | Local path for SSH/Ansible access |

### Security Isolation

- **Web Firewall** — Publicly allows ports `80`, `443`, `22`
- **DB Firewall** — Publicly allows port `22` (admin only); port `5432` is restricted to traffic originating from the Web Droplet's ID

## Deployment Flow

```
terraform apply
       ↓
[Create DB Droplet]          [Create Web Droplet]
(Ubuntu 24.04 LTS)           (Ubuntu 24.04 LTS)
       ↓                             ↓
[Hardened Firewall]          [Public Firewall]
(Port 5432 restricted)       (Ports 80, 443, 22)
       ↓                             ↓
[Ansible Play 1: DB]         [Ansible Play 2: Web]
- Install PostgreSQL 16      - Install Docker
- Apply schema.sql           - Deploy Docker Stack
- Create minitwit_user       - Connect to DB Node
```

## Troubleshooting

**Connection refused (Web → DB)**

Check that PostgreSQL is listening on all interfaces:
```bash
grep "listen_addresses" /etc/postgresql/16/main/postgresql.conf
# Expected: listen_addresses = '*'
```
Also verify the Web Droplet's IP is whitelisted for port 5432 in the DigitalOcean Firewall settings.

**503 Service Unavailable**

Nginx Proxy is running but doesn't recognize the domain. Ensure `VIRTUAL_HOST` matches your `domain_name` variable. When testing via IP, add a hosts file entry mapping `minitwit.tech` to the Web Droplet IP.

**Schema ownership errors**

`schema.sql` contains `OWNER TO minitwit_user`. If you change the database username, update the SQL file accordingly or remove the `ALTER TABLE ... OWNER` lines.

## Cleanup

Destroy the entire cluster (Web, DB, and Firewalls):

```bash
terraform destroy
```

---

*Managed with Terraform & Ansible*