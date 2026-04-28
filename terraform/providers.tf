# ============================================================================
# Terraform Provider Configuration
# ============================================================================
# This file configures the providers for both local (VirtualBox) and 
# production (DigitalOcean) deployments

terraform {
  required_version = ">= 1.0"
  
  required_providers {
    libvirt = {
      source  = "dmacvicar/libvirt"
      version = "~> 0.7"
    }
    digitalocean = {
      source  = "digitalocean/digitalocean"
      version = "~> 2.0"
    }
    local = {
      source  = "hashicorp/local"
      version = "~> 2.0"
    }
    null = {
      source  = "hashicorp/null"
      version = "~> 3.0"
    }
  }

  # Optional: Use remote state backend (e.g., S3, Terraform Cloud)
  # Uncomment and configure as needed
  # backend "s3" {
  #   bucket         = "minitwit-terraform-state"
  #   key            = "minitwit/terraform.tfstate"
  #   region         = "us-east-1"
  #   encrypt        = true
  #   dynamodb_table = "terraform-locks"
  # }
}

# ============================================================================
# DigitalOcean Provider Configuration
# ============================================================================
provider "digitalocean" {
  token = var.digitalocean_token

  # Only configure if deploying to production
  skip_wait_for_available_droplet = false
}

# ============================================================================
# Libvirt Provider Configuration (for local VirtualBox)
# ============================================================================
provider "libvirt" {
  count   = var.deployment_mode == "local" ? 1 : 0
  uri     = "qemu:///system"
  # Alternative: "qemu+ssh://user@host/system"
}

# ============================================================================
# Local Provider (for generating files)
# ============================================================================
provider "local" {
}

provider "null" {
}
