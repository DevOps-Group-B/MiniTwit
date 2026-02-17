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

/// <summary>
/// Program class is the entry point for the Chirp application.
/// It sets up the services for the application and sets up the database.
/// It also sets up the authentication with GitHub and other middleware.
/// </summary>

var builder = WebApplication.CreateBuilder(args);

// Add user secrets
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

/*
  The database path is set to the environment variable CHIRPDBPATH.
  If the environment variable is not set, the database will be created in the temp folder with the name minitwit.db.
  Otherwise, the database will be created in the path specified by the environment variable.  
*/
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
//test
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

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
    using var context = scope.ServiceProvider.GetService<ChirpDBContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Author>>();
    if (context == null) return;
    if (DbInitializer.CreateDb(context)) await DbInitializer.SeedDatabase(context, userManager);
}

//The tests use the instead of a delay to know when the server is ready
app.Logger.LogInformation("Application started and listening on port 5273");

app.Run();

public partial class Program() { }