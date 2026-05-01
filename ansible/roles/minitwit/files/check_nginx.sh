#!/bin/bash
# Health check script for Nginx
# Used by keepalived to determine if this server should remain master

if systemctl is-active --quiet nginx; then
    # Nginx is running, perform basic connectivity check
    if curl -sf http://localhost/health > /dev/null 2>&1; then
        exit 0  # Success - Nginx is healthy
    else
        exit 1  # Nginx running but not responding to health check
    fi
else
    exit 1  # Nginx is not running
fi
