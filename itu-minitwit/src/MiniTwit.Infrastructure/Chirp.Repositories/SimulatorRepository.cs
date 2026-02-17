using Chirp.Core.Models;
using Chirp.Core.Simulator;
using Chirp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Chirp.Repositories;

public class SimulatorRepository : ISimulatorRepository
{
    private readonly ChirpDBContext _dbcontext;

    public SimulatorRepository(ChirpDBContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

    public async Task<int> GetLatestAsync()
    {
        var entity = await _dbcontext.Latest.FirstOrDefaultAsync();
        return entity?.Value ?? -1; // Default to -1 if not set
    }

    public async Task UpdateLatestAsync(int? latest)
    {
        if (!latest.HasValue) return;

        var entity = await _dbcontext.Latest.FirstOrDefaultAsync();
        
        if (entity == null)
        {
            entity = new LatestEntity { Value = latest.Value };
            _dbcontext.Latest.Add(entity);
        }
        else
        {
            entity.Value = latest.Value;
            _dbcontext.Latest.Update(entity);
        }

        await _dbcontext.SaveChangesAsync();
    }
}