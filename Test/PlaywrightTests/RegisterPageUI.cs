using System.Text.RegularExpressions;
using Chirp.PlaywrightTests;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
namespace PlaywrightTests;

[Parallelizable(ParallelScope.None)]
[TestFixture]

public class RegisterPageUI : PlaywrightTestBase
{

    /// <summary>
    /// Setup before each test to test the register pages ui
    /// </summary>
    [SetUp]
    public async Task GoToRegisterPage()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Register account" }).ClickAsync(); 
        
    }
    
    // all following test will be for when users are not logged in
    /// <summary>
    /// The register page has the right title showing on the browsers tab
    /// </summary>
    [Test]
    public async Task RegisterPageLoadsAndHasCorrectTitleShows()
    {
        
        await Expect(Page).ToHaveTitleAsync(new Regex("Register"));
        
    }

    /// <summary>
    /// Register page ui text is showing
    /// </summary>
    [Test]
    public async Task LoginPageLoadsAndHasCorrectContent()
    {
        await Expect(Page.GetByText("Create a new account")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Use another service to register.")).ToBeVisibleAsync();
    }

    /// <summary>
    /// Register page has email field ui
    /// </summary>
    [Test]
    public async Task RegisterPageHasEmailFields()
    {
        await Expect(Page.GetByLabel("Email")).ToBeVisibleAsync();
    }
    
    /// <summary>
    /// Register page has password field ui
    /// </summary>
    [Test]
    public async Task RegisterPageHasPasswordField()
    {
        
        await Expect(Page.GetByLabel("Password", new() { Exact = true })).ToBeVisibleAsync();
        // exact true is used because it kept getting errors without because of the field confirm password contains the word password aswell
    }
    
    /// <summary>
    /// Register page has password confirmation field ui
    /// </summary>
    [Test]
    public async Task RegisterPageHasConfirmPasswordField()
    {
        await Expect(Page.GetByLabel("Confirm Password")).ToBeVisibleAsync();
    }

    /// <summary>
    /// Register page has register button ui
    /// </summary>
    [Test]
    public async Task RegisterPageHasRegisterButton()
    {
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Register" })).ToBeVisibleAsync();
    }
    
    /// <summary>
    /// Register page has github button ui
    /// </summary>
    [Test]
    public async Task RegisterHasGithubButton()
    {
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Github" })).ToBeVisibleAsync();
    }
    
}
