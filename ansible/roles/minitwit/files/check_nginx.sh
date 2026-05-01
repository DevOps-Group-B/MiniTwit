#!/bin/bash
# Health check script for the containerized nginx proxy.
# Used by keepalived to determine if this server should remain master.

if curl -sf http://localhost/health > /dev/null 2>&1; then
    exit 0  # Success - proxy is healthy
else
    exit 1  # Proxy is not responding
fi
