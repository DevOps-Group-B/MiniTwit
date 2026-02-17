namespace Chirp.Core.Simulator;

public interface ISimulatorRepository
{
    // Returns the current global 'latest' value
    Task<int> GetLatestAsync();

    // Updates the global 'latest' value if the new value is provided
    Task UpdateLatestAsync(int? latest);
}