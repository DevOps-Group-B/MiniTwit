using Chirp.Core.Models;
using Chirp.Core.Repositories;
using Chirp.Core.Services;
using Chirp.Core.Simulator;
using Chirp.Infrastructure;
using Chirp.Infrastructure.Chirp.Repositories;
using Chirp.Infrastructure.Chirp.Services;
using Chirp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Add HTTP logging
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestPath
        | HttpLoggingFields.RequestMethod
        | HttpLoggingFields.ResponseStatusCode;
});

string dbPath = Environment.GetEnvironmentVariable("CHIRPDBPATH") ?? Path.Combine(Path.GetTempPath(), "minitwit.db");

builder.Services.AddDbContext<ChirpDBContext>(options => options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddDefaultIdentity<Author>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 0;
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseHttpLogging();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

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
    using var context = scope.ServiceProvider.GetService<ChirpDBContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Author>>();
    if (context == null) return;
    if (DbInitializer.CreateDb(context)) await DbInitializer.SeedDatabaseAchievements(context);
    // Replace line below with line above to seed the database with dummy-data
    // if (DbInitializer.CreateDb(context)) await DbInitializer.SeedDatabase(context, userManager);
}

app.Logger.LogInformation("Application started and listening on port 5273");

app.Run();

public partial class Program() { }