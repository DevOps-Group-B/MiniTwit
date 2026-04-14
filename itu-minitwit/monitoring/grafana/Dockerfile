FROM grafana/grafana:13.0.0

ENV PROMETHEUS_HOST='host.docker.internal:9090'

COPY ./provisioning/dashboards /etc/grafana/provisioning/dashboards
COPY ./provisioning/datasources /etc/grafana/provisioning/datasources

COPY ./dashboards /etc/grafana/dashboards

EXPOSE 3000
