using Chirp.Core.Models;
using Chirp.Core.Repositories;
using Chirp.Core.Services;
using Chirp.Core.Simulator;
using Chirp.Infrastructure;
using Chirp.Infrastructure.Chirp.Repositories;
using Chirp.Infrastructure.Chirp.Services;
using Chirp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Prometheus;
using Minitwit.Services;

/// <summary>
/// Program class is the entry point for the Chirp application.
/// It sets up the services for the application and sets up the database.
/// It also sets up the authentication with GitHub and other middleware.
/// </summary>

// Bootstrap logger active until full Serilog config is read from appsettings
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Replace the default .NET logging with Serilog, configured from appsettings.json
builder.Host.UseSerilog((context, services, config) =>
    config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

// Add user secrets
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}
// Add services to the container.
builder.Services.AddSingleton<IMetricsService, MetricsService>();
builder.Services.AddRazorPages();
builder.Services.AddControllers();

/*
  Database configuration:
  - In production: Uses PostgreSQL via POSTGRES_CONNECTION_STRING environment variable
  - In development: Falls back to SQLite in temp folder if CONNECTION_STRING is not set
*/
string? connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");

if (!string.IsNullOrEmpty(connectionString))
{
    // Production: Use PostgreSQL
    builder.Services.AddDbContext<ChirpDBContext>(options => 
        options.UseNpgsql(connectionString));
}
else
{
    // Development: Fall back to SQLite
    string dbPath = Path.Combine(Path.GetTempPath(), "minitwit.db");
    builder.Services.AddDbContext<ChirpDBContext>(options => 
        options.UseSqlite($"Data Source={dbPath}"));
}

builder.Services.AddDefaultIdentity<Author>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 0;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
}).AddEntityFrameworkStores<ChirpDBContext>();

builder.Services.AddScoped<IAchievementRepository, AchievementRepository>();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICheepService, CheepService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<ISimulatorRepository, SimulatorRepository>();

var clientId = builder.Configuration["authentication_github_clientId"];
var clientSecret = builder.Configuration["authentication_github_clientSecret"];

if (clientId != null && clientSecret != null)
{
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultChallengeScheme = "GitHub";
        })
        .AddCookie()
        .AddGitHub(o =>
        {
            o.ClientId = clientId;
            o.ClientSecret = clientSecret;
            o.CallbackPath = "/signin-github";
            o.Scope.Add("user:email");
        });   
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseHttpMetrics();

// Log every HTTP request: method, path, client IP, status code, elapsed ms
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} from {RemoteIpAddress} => {StatusCode} in {Elapsed:0}ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RemoteIpAddress",
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
    };
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapMetrics();
app.MapControllers();
app.MapRazorPages();

/*
  This is a custom redirect policy for the /Register and /Login page
  If the user is authenticated and tries to go to the /Register or /Login page they will be redirected to the home page
*/
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/Identity/Account/Register") && (context.User.Identity?.IsAuthenticated ?? true))
    {
        context.Response.Redirect("/");
    }
    else if (context.Request.Path.StartsWithSegments("/Identity/Account/Login") && (context.User.Identity?.IsAuthenticated ?? true))
    {
        context.Response.Redirect("/");
    }
    else
    {
        await next();
    }
});


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    using var context = services.GetService<ChirpDBContext>();
    var userManager = services.GetRequiredService<UserManager<Author>>();
    if (context == null) return;
    if (DbInitializer.CreateDb(context)) await DbInitializer.SeedDatabaseAchievements(context);
    // Replace with line above to seed the database with dummy-data
    // if (DbInitializer.CreateDb(context)) await DbInitializer.SeedDatabase(context, userManager);

    var cheepService = services.GetRequiredService<ICheepService>();
    var metricsService = services.GetRequiredService<IMetricsService>();

    try 
    {
        var totalUsers = await userManager.Users.CountAsync();
        var totalCheeps = await cheepService.GetTotalCheepsAsync();
        var cheepsPerUser = totalUsers > 0 ? (float)totalCheeps / totalUsers : 0;
        
        metricsService.SetTotalUsers(totalUsers);
        metricsService.SetTotalCheeps(totalCheeps);
        metricsService.SetCheepsPerUsers(cheepsPerUser);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Failed to initialize metrics on startup");
    }
}

//The tests use the instead of a delay to know when the server is ready
app.Logger.LogInformation("Application started and listening on port 5273");

app.Run();

public partial class Program() { }