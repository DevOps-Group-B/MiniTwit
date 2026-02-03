using System.Text.RegularExpressions;
using Chirp.PlaywrightTests;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace PlaywrightTests;


[Parallelizable(ParallelScope.None)]
[TestFixture]
public class LoginPageUI : PlaywrightTestBase
{
    
    /// <summary>
    /// Setup before each test, to test the login ui page
    /// </summary>
    [SetUp]
    public async Task GoToLoginPage()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync(); 
        
    }
    
    // all following test will be for when users are not logged in
    
    /// <summary>
    /// The login page has the right title showing on the browsers tab
    /// </summary>
    [Test]
    public async Task LoginPageLoadsAndHasCorrectTitle()
    {
        
        await Expect(Page).ToHaveTitleAsync(new Regex("Log in"));
    }

    /// <summary>
    /// Login page ui text is showing
    /// </summary>
    [Test]
    public async Task LoginPageLoadsAndHasCorrectContent()
    {
        await Expect(Page.GetByText("Use a local account to log in.")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Use another service to log in.")).ToBeVisibleAsync();
    }

    /// <summary>
    /// Login page has ui for login button
    /// </summary>
    [Test]
    public async Task LoginPageHasLoginButton()
    {
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Log in" })).ToBeVisibleAsync();
    
    }
    
    /// <summary>
    /// Login page has ui for github login button
    /// </summary>
    [Test]
    public async Task LoginPageHasGithubLoginButton()
    {
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "GitHub" })).ToBeVisibleAsync();
    }

    /// <summary>
    /// Login page input field ui is showing
    /// </summary>
    [Test]
    public async Task LoginPageHasInputFields()
    {
        // email and password fields/labels
        await Expect(Page.GetByLabel("Email")).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("Password")).ToBeVisibleAsync();
    }
    
    /// <summary>
    /// Login page checkbox ui showing
    /// </summary>
    [Test]
    public async Task LoginPageHasRememberMeCheckbox()
    {
        // the checkbox
        await Expect(Page.GetByRole(AriaRole.Checkbox, new() { Name = "Remember me?" })).ToBeVisibleAsync();
    }
    
    /// <summary>
    /// Login page has links to forget password
    /// resend email confirmation
    /// register as new user
    /// </summary>
    [Test]
    public async Task LoginPageHasLinks()
    {
        // the 3 links under login button
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Forgot your password?" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Register as a new user" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Resend email confirmation" })).ToBeVisibleAsync();
    }
    
}