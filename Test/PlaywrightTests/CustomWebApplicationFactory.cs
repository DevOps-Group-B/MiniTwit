using System.Data.Common;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chirp.PlaywrightTests;

/// <summary>
/// This class is used to host a connection to run the playwright tests in browser without having to run the program
/// </summary>

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private IHost? _kestrelHost;
    private SqliteConnection? _keepAlive;
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // 1. Service configuration for BOTH hosts
        builder.ConfigureServices(services =>
        {
            services.AddLogging(logging => logging.ClearProviders());

            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CheepDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            var connectionString = "Data Source=file:chirp-tests?mode=memory&cache=shared";
            _keepAlive = new SqliteConnection(connectionString);
            _keepAlive.Open();

            services.AddDbContext<CheepDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });
        });

        // 2. Let WebApplicationFactory create the "normal" TestServer host.
        //    This is what its internals expect and will cast to TestServer.
        var testHost = base.CreateHost(builder);

        // 3. Configure a REAL HTTP server using Kestrel on a FIXED URL.
        //    Pick any port that’s unlikely to collide, e.g. 5005.
        const string kestrelUrl = "http://127.0.0.1:5005";

        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseKestrel();
            webHostBuilder.UseUrls(kestrelUrl);
        });

        // 4. Start the Kestrel host
        try
        {
            _kestrelHost = builder.Build();
            _kestrelHost.Start();
        }
        catch (Exception ex)
        {
            TestContext.WriteLine(ex.ToString());
            throw;
        }

        // 5. Tell WebApplicationFactory clients to use this URL
        ClientOptions.BaseAddress = new Uri(kestrelUrl);

        // 6. Run migrations on the Kestrel host
        using (var scope = _kestrelHost.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CheepDbContext>();
            context.Database.EnsureCreated();
        }

        // 7. Return the TestServer host (keeps WebApplicationFactory happy)
        return testHost;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _kestrelHost?.Dispose();
            _keepAlive?.Dispose();
        }

        base.Dispose(disposing);
    }
}
