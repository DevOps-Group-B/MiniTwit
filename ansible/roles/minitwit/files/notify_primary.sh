#!/bin/bash
# Notification script when transitioning to MASTER state
# This script assigns the floating IP to the current droplet via the DigitalOcean API

FLOATING_IP="138.68.126.154"
LOGFILE="/var/log/keepalived-notify.log"
TOKEN_FILE="/etc/minitwit_do_token"
METADATA_URL="http://169.254.169.254/metadata/v1/id"

log_message() {
    echo "$(date '+%Y-%m-%d %H:%M:%S'): $1" >> "$LOGFILE"
}

log_message "Transitioned to MASTER state, attempting to assign floating IP $FLOATING_IP to this server"

# Check if token file exists
if [ ! -f "$TOKEN_FILE" ]; then
    log_message "ERROR: DigitalOcean API token not found at $TOKEN_FILE"
    exit 1
fi

# Read the token
API_TOKEN=$(cat "$TOKEN_FILE")

# Get the current droplet ID from DigitalOcean metadata
DROPLET_ID=$(curl -s --connect-timeout 5 -m 5 "$METADATA_URL")

if [ -z "$DROPLET_ID" ]; then
    log_message "ERROR: Failed to retrieve droplet ID from metadata endpoint"
    exit 1
fi

log_message "Current droplet ID: $DROPLET_ID"

# Assign the floating IP to this droplet
RESPONSE=$(curl -s -X POST \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $API_TOKEN" \
    -d "{\"type\":\"assign\",\"droplet_id\":$DROPLET_ID}" \
    "https://api.digitalocean.com/v2/floating_ips/$FLOATING_IP/actions")

# Check if the API call was successful
if echo "$RESPONSE" | grep -q '"status":"in-progress"'; then
    log_message "Successfully assigned floating IP $FLOATING_IP to droplet $DROPLET_ID"
elif echo "$RESPONSE" | grep -q '"already_assigned"'; then
    log_message "Floating IP $FLOATING_IP was already assigned to this droplet"
else
    log_message "ERROR: Failed to assign floating IP. Response: $RESPONSE"
    exit 1
fi
