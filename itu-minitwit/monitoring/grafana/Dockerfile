FROM grafana/grafana:12.1

ENV PROMETHEUS_HOST='host.docker.internal:9090'

COPY ./provisioning/dashboards /etc/grafana/provisioning/dashboards
COPY ./provisioning/datasources /etc/grafana/provisioning/datasources

COPY ./dashboards/ /var/lib/grafana/dashboards/

EXPOSE 3000
