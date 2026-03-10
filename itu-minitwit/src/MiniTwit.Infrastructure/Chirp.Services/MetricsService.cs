using Prometheus;

namespace Minitwit.Services;

public class MetricsService : IMetricsService
{
    private static readonly Gauge TotalUsers = Metrics.CreateGauge(
        "minitwit_total_users", 
        "The total number of users in the system."
    );

    public MetricsService()
    {
        // Force the metrics to appear in the /metrics list immediately
        TotalUsers.Set(0);
    }

    public void SetTotalUsers(int value) => TotalUsers.Set(value);
}