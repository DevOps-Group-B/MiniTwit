# MiniTwit — Agent Documentation

Purpose: a concise developer-ops overview of the project architecture, components, and dependencies. Use this as a single reference for running, provisioning, and extending the MiniTwit devops school project.

**Project Overview**
- **Name:** MiniTwit (ITU DevOps exercise)
- **Language / Framework:** C# / ASP.NET Core (TargetFramework: net9.0)
- **Repository layout:** a solution under `itu-minitwit/` containing three main projects: `MiniTwit.Web`, `MiniTwit.Core`, `MiniTwit.Infrastructure` plus test projects and tooling.

**Architecture (high level)**
- Presentation/API: `MiniTwit.Web` — ASP.NET Core web application serving pages and API endpoints.
- Domain: `MiniTwit.Core` — DTOs, models and domain services.
- Data/Infra: `MiniTwit.Infrastructure` — Entity Framework Core, DB access and initialization.
- Monitoring: Prometheus (metrics), Grafana (dashboards), Loki (logs), Alloy (auxiliary log indexer).
- Provisioning & Infra: Docker Compose for local stack, Vagrant + Ansible for local VM provisioning, Terraform for production DigitalOcean provisioning.

**Key files**
- Solution root: [MiniTwit.sln](itu-minitwit/MiniTwit.sln#L1-L1)
- Web app: [itu-minitwit/src/MiniTwit.Web/MiniTwit.Web.csproj](itu-minitwit/src/MiniTwit.Web/MiniTwit.Web.csproj#L1-L20)
- Core: [itu-minitwit/src/MiniTwit.Core/MiniTwit.Core.csproj](itu-minitwit/src/MiniTwit.Core/MiniTwit.Core.csproj#L1-L20)
- Infrastructure: [itu-minitwit/src/MiniTwit.Infrastructure/MiniTwit.Infrastructure.csproj](itu-minitwit/src/MiniTwit.Infrastructure/MiniTwit.Infrastructure.csproj#L1-L20)
- Main Docker Compose: [docker-compose.yml](docker-compose.yml#L1-L40)
- App Dockerfile: [itu-minitwit/Dockerfile](itu-minitwit/Dockerfile#L1-L40)
- Monitoring configs: `itu-minitwit/monitoring/` (Prometheus/Grafana/Loki)
  - Prometheus config: [itu-minitwit/monitoring/prometheus/prometheus.yml](itu-minitwit/monitoring/prometheus/prometheus.yml#L1-L40)
  - Grafana Dockerfile: [itu-minitwit/monitoring/grafana/Dockerfile](itu-minitwit/monitoring/grafana/Dockerfile#L1-L40)
  - Loki config: [itu-minitwit/monitoring/loki/config.yml](itu-minitwit/monitoring/loki/config.yml#L1-L40)
- Provisioning: [ansible/playbook.yml](ansible/playbook.yml#L1-L40), [Vagrantfile](Vagrantfile#L1-L40), [terraform/main.tf](terraform/main.tf#L1-L40)

**Dependencies (by project)**
- `MiniTwit.Core` (net9.0)
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.10
  - prometheus-net.AspNetCore 8.2.1

- `MiniTwit.Web` (net9.0)
  - Project references: `MiniTwit.Core`, `MiniTwit.Infrastructure` (see csproj)
  - Key PackageReferences (seen in csproj):
    - AspNet.Security.OAuth.GitHub 8.0.0
    - Microsoft.Build 17.x
    - prometheus-net.AspNetCore 8.2.1
    - Serilog.AspNetCore 9.0.0
    - Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.10
    - Microsoft.AspNetCore.Identity.UI 8.0.10
    - Entity Framework packages: Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.Sqlite 8.0.x, Microsoft.EntityFrameworkCore.Tools
    - Npgsql.EntityFrameworkCore.PostgreSQL 8.0.x
    - Microsoft.Playwright 1.49.0 (used by integration/end-to-end tests)

- `MiniTwit.Infrastructure` (net9.0)
  - Microsoft.Data.Sqlite 8.0.8
  - Microsoft.EntityFrameworkCore.Sqlite 8.0.8
  - Npgsql.EntityFrameworkCore.PostgreSQL 8.0.8
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.10

**Containers & Images**
- App image is built from `mcr.microsoft.com/dotnet/sdk:9.0` (build) and runs on `mcr.microsoft.com/dotnet/aspnet:9.0-noble-chiseled` as runtime ([itu-minitwit/Dockerfile](itu-minitwit/Dockerfile#L1-L40)).
- Grafana base image: `grafana/grafana:12.4` with pre-provisioned dashboards.
- Loki image: `grafana/loki:3.5.0` (used as-is) and Alloy image `grafana/alloy:latest`.
- Docker Compose wires containers and ports in `docker-compose.yml` (web app port mapping 8080:5273, grafana 3000, prometheus 9090, loki 3100).

**Monitoring stack**
- Prometheus scrapes metrics from the app (`minitwit:5273`) and hosts its own UI at port 9090. See [itu-minitwit/monitoring/prometheus/prometheus.yml](itu-minitwit/monitoring/prometheus/prometheus.yml#L1-L40).
- Grafana runs on port 3000 and is configured to load dashboards and datasources from `itu-minitwit/monitoring/grafana/provisioning` and `dashboards/`.
- Loki captures logs and is configured via `itu-minitwit/monitoring/loki/config.yml`.
- Alloy is included for additional log analysis (requires host path to Docker containers for indexing—configured in `docker-compose.yml` via `ALLOY_DOCKER_CONTAINERS_PATH` environment).

**Infrastructure & Provisioning**
- Local dev: `docker-compose up --build` will start the web app and monitoring stack. See [docker-compose.yml](docker-compose.yml#L1-L40).
- Vagrant (local VM): `vagrant up` provisions a VM with Ansible (see `Vagrantfile` and `ansible/playbook.yml`). Vagrant forwards ports for web (8080), grafana (3000), prometheus (9090), loki (3100).
- Ansible role: `ansible/roles/minitwit` contains tasks/templates to deploy the container image, set environment variables, etc. The top-level playbook is [ansible/playbook.yml](ansible/playbook.yml#L1-L40).
- Production: Terraform configuration in `terraform/` targets DigitalOcean droplets, creates firewall rules and can generate an Ansible inventory. See [terraform/main.tf](terraform/main.tf#L1-L40).

**Environment variables & secrets**
- `docker-compose.yml` expects `POSTGRES_CONNECTION_STRING` and `GRAFANA_ADMIN_USER`/`GRAFANA_ADMIN_PASSWORD` to be provided via environment or `.env` file.
- Vagrant/Ansible also accept variables via `extra_vars` or environment variables (see `Vagrantfile` and `ansible/playbook.yml`).

**Testing**
- Tests projects are under `itu-minitwit/tests/`:
  - Unit tests: `tests/UnitTest`
  - Integration tests: `tests/IntegrationTest`
  - End2End: `tests/End2end` (Playwright-driven)
- Playwright is included as a package reference in `MiniTwit.Web` for browser-based tests.

**How to run (quick)**
1. Docker Compose (fastest local dev):
   - Provide environment variables (example `.env`) for `POSTGRES_CONNECTION_STRING`, Grafana credentials, etc.
   - Build & start:
     ```
     docker-compose up --build
     ```
   - Access:
     - App: http://localhost:8080
     - Grafana: http://localhost:3000
     - Prometheus: http://localhost:9090

2. Vagrant + Ansible (VM-based local dev):
   - `vagrant up` (this installs Ansible on the guest and runs `ansible/playbook.yml`)
   - Ports are forwarded; use same access URLs above.

3. Production (high-level):
   - Use `terraform` under `terraform/` to create a DigitalOcean droplet, then run Ansible against generated inventory to provision containers.

**Useful commands**
- Build and run with Docker Compose:
  - `docker-compose up --build` (development stack)
- Rebuild only the app image:
  - `docker-compose build minitwit`
- Run tests (dotnet):
  - `dotnet test itu-minitwit/tests/UnitTest`

**Notes & Recommendations**
- DB: The project supports both SQLite (for local testing) and PostgreSQL (Npgsql packages are present); provide `POSTGRES_CONNECTION_STRING` to use Postgres.
- Security: Grafana default admin credentials are set via env vars—change them in production.
- Monitoring: The app exposes Prometheus metrics via `prometheus-net.AspNetCore` (check middleware/Program.cs for metric endpoints if you need custom scraping).
- Upgrade path: target framework is `net9.0`; keep docker base images and EF Core package versions aligned.

**Where to look next (developer pointers)**
- App entry & configuration: [itu-minitwit/src/MiniTwit.Web/Program.cs](itu-minitwit/src/MiniTwit.Web/Program.cs#L1-L120)
- DB initialization: [itu-minitwit/src/MiniTwit.Infrastructure/DbInitializer.cs](itu-minitwit/src/MiniTwit.Infrastructure/DbInitializer.cs#L1-L200)
- Ansible role: [ansible/roles/minitwit/tasks/main.yml](ansible/roles/minitwit/tasks/main.yml#L1-L200)

---
Generated by the project Agent: concise architecture & ops summary. If you want, I can:
- add a runnable `.env.example` with required variables,
- generate quick start scripts (`make`, `ps1`), or
- run the app locally (build + docker-compose) and verify endpoints.
