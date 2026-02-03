using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Chirp.PlaywrightTests;


/// <summary>
/// This class is using the WebApplicationFactory class to create a client so playwright can run test
/// in browser, without having us to run the application.
/// </summary>
[TestFixture]
public class PlaywrightTestBase : PageTest
{
    public CustomWebApplicationFactory _factory = null!;
    public string BaseUrl { get; set; } = null!;

    [SetUp]
    public async Task GlobalSetup()
    {
        _factory = new CustomWebApplicationFactory();
        var client = _factory.CreateClient(); // starts everything

        BaseUrl = _factory.ClientOptions.BaseAddress.ToString();
        //TestContext.WriteLine($"BaseUrl: {BaseUrl}");

        // Quick check that the app responds
        var response = await client.GetAsync("/");
        //TestContext.WriteLine($"GET / => {(int)response.StatusCode}");
        response.EnsureSuccessStatusCode();

        Page.SetDefaultTimeout(60000);
    }

    [TearDown]
    public async Task GlobalTeardown()
    {
        await _factory.DisposeAsync();
    }
}