#!/bin/bash
# Notification script when entering FAULT state
echo "$(date): Entered FAULT state, health check failed" >> /var/log/keepalived-notify.log
