using Chirp.Domain;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure;

public class CheepRepository : ICheepRepository
{
    private readonly CheepDbContext _dbContext;

    public CheepRepository(CheepDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    /// <summary>
    /// Gets a paginated list of cheeps ordered by most recent first.
    /// </summary>
    /// <param name="page">The page number to retrieve.</param>
    /// <returns>A list of cheeps for the specified page.</returns>
    public async Task<List<CheepDTO>> ReadCheeps(int page)
    {
        const int pageSize = 32;
        var skip = Math.Max(0, (page - 1) * pageSize);

        var cheeps = await _dbContext.Cheeps
        .AsNoTracking()
        .OrderByDescending(c => c.TimeStamp)
        .Include(c => c.User)
        .Skip(skip)
        .Take(pageSize)
        .Select(c => new CheepDTO
        {
            CheepId = c.CheepId,
            Text = c.Text,
            User = c.User,
            Likes = c.Likes,
            TimeStamp = c.TimeStamp
        })
        .ToListAsync();
        return cheeps;
    }

    /// <summary>
    /// Converts a CheepDTO into a cheep, and adds it to the database. 
    /// </summary>
    /// <param name="message">The CheepDTO to add.</param>
    public async Task InsertNewCheepAsync(CheepDTO message)
    {
        var newCheep = new Cheep
        {
            Text = message.Text,
            UserId = message.User.Id,
            User = message.User,
            TimeStamp = message.TimeStamp,
        };
        _dbContext.Cheeps.Add(newCheep);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves a paginated list of cheeps written by a specified user, ordered by most recent first.
    /// </summary>
    /// <param name="user">The user whose cheeps to retrieve.</param>
    /// <param name="page">The page number to retrieve.</param>
    /// <returns>A list of <see cref="CheepDTO"/> objects authored by the specified user.</returns>
    public async Task<List<CheepDTO>> getCheepsFromUser(User user, int page)
    {
        const int pageSize = 32;
        var skip = Math.Max(0, (page - 1) * pageSize);

        var cheeps = await _dbContext.Cheeps
            .AsNoTracking()
            .OrderByDescending(c => c.TimeStamp)
            .Include(c => c.User)
            .Skip(skip)
            .Take(pageSize)
            .Where(c => c.User == user)
            .Select(c => new CheepDTO
            {
                CheepId = c.CheepId,
                Text = c.Text,
                User = c.User,
                Likes = c.Likes,
                TimeStamp = c.TimeStamp
            })
            .ToListAsync();
        return cheeps;
    }

    /// <summary>
    /// Retrieves a paginated list of cheeps authored by the specified user, ordered by most recent first.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose cheeps to retrieve.</param>
    /// <param name="page">The page number to retrieve.</param>
    /// <returns>A list of <see cref="CheepDTO"/> objects authored by the specified userId.</returns>
    public async Task<List<CheepDTO>> getCheepsFromUserId(string userId, int page)
    {
        const int pageSize = 32;
        var skip = Math.Max(0, (page - 1) * pageSize);

        var cheeps = await _dbContext.Cheeps
            .AsNoTracking()
            .OrderByDescending(c => c.TimeStamp)
            .Include(c => c.User)
            .Skip(skip)
            .Take(pageSize)
            .Where(c => c.UserId == userId)
            .Select(c => new CheepDTO
            {
                CheepId = c.CheepId,
                Text = c.Text,
                User = c.User,
                Likes = c.Likes,
                TimeStamp = c.TimeStamp
            })
            .ToListAsync(); 

        return cheeps;
    }

    /// <summary>
    /// Adds a like from the specified user to the cheep with the given ID.
    /// </summary>
    /// <param name="currentUser">The user who is liking the cheep.</param>
    /// <param name="cheepId">The unique identifier of the cheep to like.</param>
    public async Task<string> LikeCheep(User currentUser, int cheepId)
    {
        List<string> Likes = new List<string>();
        Likes.Add(currentUser.Id);

        var cheep = await _dbContext.Cheeps.FindAsync(cheepId);
        if (cheep == null)
        {
            return "Cheep not found";
        }
        if (cheep.Likes == null)
        {
            cheep.Likes = Likes;
        }
        else if (!cheep.Likes.Contains(currentUser.Id))
        {
            cheep.Likes.Add(currentUser.Id);
        }
        else
        {
            Console.WriteLine("Error when liking");
        }
        await _dbContext.SaveChangesAsync();

        return "Success";
    }
    /// <summary>
    /// Removes a like from a cheep given by the specified user.
    /// </summary>
    /// <param name="currentUser">The user who is unlike the cheep.</param>
    /// <param name="cheepId">The unique identifier of the cheep to unlike.</param>
    public async Task<string> UnLikeCheep(User currentUser, int cheepId)
    {
        var cheep = await _dbContext.Cheeps.FindAsync(cheepId);
        if (cheep == null)
        {
            return "Cheep not found";
        }
        if (cheep.Likes != null && cheep.Likes.Contains(currentUser.Id))
        {
            cheep.Likes.Remove(currentUser.Id);
            await _dbContext.SaveChangesAsync();
            return "Success";
        }
        return "User has not liked this cheep";
    }
}