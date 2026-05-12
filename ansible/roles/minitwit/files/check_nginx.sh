#!/bin/bash
docker inspect --format='{{.State.Running}}' itu-minitwit-nginx-proxy 2>/dev/null | grep -q "true"