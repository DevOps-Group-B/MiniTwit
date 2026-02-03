using Chirp.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class SqliteInMemoryDbFixture : IDisposable
{
    private readonly SqliteConnection _keepAliveConnection;
    public DbContextOptions<CheepDbContext> Options { get; }

    public SqliteInMemoryDbFixture()
    {
        _keepAliveConnection = new SqliteConnection("Data Source=:memory:;Cache=Shared");
        _keepAliveConnection.Open();

        Options = new DbContextOptionsBuilder<CheepDbContext>()
            .UseSqlite(_keepAliveConnection)
            .EnableSensitiveDataLogging()
            .Options;

        using var ctx = CreateContext();
        ctx.Database.EnsureDeleted();
        ctx.Database.EnsureCreated(); // Use EnsureCreated instead of Migrate
    }

    public CheepDbContext CreateContext() => new CheepDbContext(Options);

    public void ResetDatabase()
    {
        using var ctx = CreateContext();
        ctx.Database.EnsureDeleted();
        ctx.Database.EnsureCreated(); // Use EnsureCreated
    }

    public void Dispose()
    {
        _keepAliveConnection.Close();
        _keepAliveConnection.Dispose();
    }
}