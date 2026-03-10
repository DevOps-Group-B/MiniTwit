namespace Minitwit.Services;

public interface IMetricsService
{
    void SetTotalUsers(int value);
    void SetTotalCheeps(int value);
    void SetCheepsPerUsers(int value);
}