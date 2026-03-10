using Prometheus;

namespace Minitwit.Services;

public class MetricsService : IMetricsService
{
    private static readonly Gauge LatestValueGauge = Metrics.CreateGauge(
        "minitwit_sim_latest_value", 
        "The latest processed command ID from the simulator."
    );

    public MetricsService()
    {
        // Force the metrics to appear in the /metrics list immediately
        LatestValueGauge.Set(0);
    }

    public void SetLatest(int value) => LatestValueGauge.Set(value);
}