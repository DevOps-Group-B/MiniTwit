#!/bin/bash
# Notification script when transitioning to BACKUP state
echo "$(date): Transitioned to BACKUP state, floating IP 138.68.126.154 is now on the other server" >> /var/log/keepalived-notify.log
