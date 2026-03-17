using Prometheus;

namespace Minitwit.Services;

public class MetricsService : IMetricsService
{
    private static readonly Gauge TotalUsers = Metrics.CreateGauge(
        "minitwit_total_users",
        "The total number of users in the system."
    );

    private static readonly Gauge TotalCheeps = Metrics.CreateGauge(
        "minitwit_total_cheeps",
        "The total number of cheeps in the system."
    );

    private static readonly Gauge CheepsPerUser = Metrics.CreateGauge(
        "minitwit_cheeps_per_user",
        "The average number of cheeps per user."
    );

    public MetricsService()
    {
        // Force the metrics to appear in the /metrics list immediately
        TotalUsers.Set(0);
        TotalCheeps.Set(0);
        CheepsPerUser.Set(0);
    }

    public void SetTotalUsers(int value) => TotalUsers.Set(value);
    public void SetTotalCheeps(int value) => TotalCheeps.Set(value);
    public void SetCheepsPerUsers(float value) => CheepsPerUser.Set(value);
}
