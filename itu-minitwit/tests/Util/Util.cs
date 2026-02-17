using Chirp.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Util;
/// <summary>
/// Util class is for creating an in-memory database for testing used in unit tests
/// </summary>
public class Util
{
   
    public static async Task<ChirpDBContext> CreateInMemoryDatabase()
    {
        // Create a new database in memory
        var connection = new SqliteConnection("Filename=:memory:");
        await connection.OpenAsync();
        var builder = new DbContextOptionsBuilder<ChirpDBContext>().UseSqlite(connection);

        var context = new ChirpDBContext(builder.Options);
        await context.Database.EnsureCreatedAsync(); 

       
        await context.SaveChangesAsync();

        return context;
    }
}