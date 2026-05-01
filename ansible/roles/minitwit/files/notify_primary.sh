#!/bin/bash
# Notification script when transitioning to MASTER state
echo "$(date): Transitioned to MASTER state, floating IP 138.68.126.154 is now bound to this server" >> /var/log/keepalived-notify.log
