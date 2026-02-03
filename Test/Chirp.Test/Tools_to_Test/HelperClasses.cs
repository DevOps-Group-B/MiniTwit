using Chirp.Domain;

namespace Chirp.Tests.Tools_to_Test;

/// <summary>
/// Provides helper methods for generating randomized <see cref="User"/>, <see cref="Cheep"/>,
/// and <see cref="CheepDTO"/> instances for testing purposes.
/// </summary>
public class HelperClasses
{
    /// <summary>
    /// Creates a randomized <see cref="User"/> object with a unique ID,
    /// a random name, and a corresponding email address.
    /// </summary>
    /// <returns>A randomly generated <see cref="User"/> instance.</returns>
    public static User createRandomUser()
    {
        var name = InputFuzzers.RandomString(100);
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Email = $"{name}@example.com",
            Cheeps = new List<Cheep>(),
            
        };
        return user;
    }

    /// <summary>
    /// Creates a randomized <see cref="Cheep"/> instance associated with the specified user.
    /// </summary>
    /// <param name="user">The user who authored the cheep.</param>
    /// <returns>A randomly generated <see cref="Cheep"/> instance.</returns>
    public static Cheep createRandomCheep(User user)
    {
        var cheep = new Cheep
        {
            Text = InputFuzzers.RandomSentence(),
            User = user,
            TimeStamp = DateTime.UtcNow,
            UserId = user.Id,
        };
        return cheep;
    }
    
    /// <summary>
    /// Creates a randomized <see cref="CheepDTO"/> instance associated with the specified user.
    /// </summary>
    /// <param name="user">The user who authored the cheep DTO.</param>
    /// <returns>A randomly generated <see cref="CheepDTO"/> instance.</returns>
    public static CheepDTO createRandomCheepDTO(User user)
    {
        var cheep = new CheepDTO
        {
            Text = InputFuzzers.RandomSentence(),
            User = user,
            TimeStamp = DateTime.UtcNow,
        };
        return cheep;
    }
}