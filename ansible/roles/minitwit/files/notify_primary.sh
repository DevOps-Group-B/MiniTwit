#!/bin/bash
# Notification script when transitioning to MASTER state.
# This follows the DigitalOcean Reserved IP pattern with a metadata check and retries.

LOGFILE="/var/log/keepalived-notify.log"
TOKEN_FILE="/etc/minitwit_do_token"
FLOATING_IP_FILE="/etc/minitwit_floating_ip"
METADATA_ID_URL="http://169.254.169.254/metadata/v1/id"
METADATA_RESERVED_IP_URL="http://169.254.169.254/metadata/v1/reserved_ip/ipv4/active"

log_message() {
    echo "$(date '+%Y-%m-%d %H:%M:%S'): $1" >> "$LOGFILE"
}

if [ ! -f "$FLOATING_IP_FILE" ]; then
    log_message "ERROR: Floating IP file not found at $FLOATING_IP_FILE"
    exit 1
fi

RESERVED_IP=$(cat "$FLOATING_IP_FILE")
if [ -z "$RESERVED_IP" ]; then
    log_message "ERROR: Floating IP value is empty in $FLOATING_IP_FILE"
    exit 1
fi

log_message "Transitioned to MASTER state, checking Reserved IP $RESERVED_IP"

if [ ! -f "$TOKEN_FILE" ]; then
    log_message "ERROR: DigitalOcean API token not found at $TOKEN_FILE"
    exit 1
fi

API_TOKEN=$(cat "$TOKEN_FILE")
DROPLET_ID=$(curl -s --connect-timeout 5 -m 5 "$METADATA_ID_URL")
HAS_RESERVED_IP=$(curl -s --connect-timeout 5 -m 5 "$METADATA_RESERVED_IP_URL")

if [ -z "$DROPLET_ID" ]; then
    log_message "ERROR: Failed to retrieve droplet ID from metadata endpoint"
    exit 1
fi

log_message "Current droplet ID: $DROPLET_ID"

if [ "$HAS_RESERVED_IP" = "true" ]; then
    log_message "Reserved IP $RESERVED_IP is already active on this droplet"
    exit 0
fi

n=0
while [ "$n" -lt 10 ]; do
    RESPONSE=$(curl -s -X POST \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $API_TOKEN" \
        -d "{\"type\":\"assign\",\"droplet_id\":$DROPLET_ID}" \
        "https://api.digitalocean.com/v2/floating_ips/$RESERVED_IP/actions")

    if echo "$RESPONSE" | grep -q '"status":"in-progress"'; then
        log_message "Successfully requested Reserved IP $RESERVED_IP assignment to droplet $DROPLET_ID"
        exit 0
    fi

    if echo "$RESPONSE" | grep -q 'already_assigned'; then
        log_message "Reserved IP $RESERVED_IP was already assigned to this droplet"
        exit 0
    fi

    n=$((n + 1))
    log_message "Reserved IP assignment attempt $n failed, retrying in 3 seconds. Response: $RESPONSE"
    sleep 3
done

log_message "ERROR: Failed to assign Reserved IP $RESERVED_IP after 10 attempts"
exit 1
