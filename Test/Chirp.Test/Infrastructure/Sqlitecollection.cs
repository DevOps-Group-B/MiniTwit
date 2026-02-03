using Xunit;

namespace Chirp.Tests.Infrastructure
{
    [CollectionDefinition("sqlite-db")]
    public class SqliteCollection : ICollectionFixture<SqliteInMemoryDbFixture>
    {
    }
}