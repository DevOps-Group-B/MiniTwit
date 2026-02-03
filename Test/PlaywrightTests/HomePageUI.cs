using System.Text.RegularExpressions;
using Microsoft.Playwright.NUnit;
using Chirp.PlaywrightTests;

namespace PlaywrightTests;
[Parallelizable(ParallelScope.None)]
[TestFixture]
public class HomePageUI : PlaywrightTestBase
{
    // all following test will be for when users are not logged in
    
    /// <summary>
    /// The homepage has the right title showing on the browsers tab
    /// </summary>
    [Test]
    public async Task HomePageLoadsAndHasCorrectTitle()
    {
        await Page.GotoAsync(BaseUrl);
        
        await Expect(Page).ToHaveTitleAsync(new Regex("BBL CHIRP!"));
    }

    /// <summary>
    /// The public timeline page is shown when going to root endpoint
    /// </summary>
    [Test]
    public async Task HomePageShowPublicTimeline()
    {

        await Page.GotoAsync(BaseUrl);

        await Expect(Page.GetByText("Public Timeline")).ToBeVisibleAsync();
    }

    /// <summary>
    /// Testing the ui elements for navbar is showing
    /// </summary>
    [Test]
    public async Task HomePageNavBarHasCorrectButtons ()
    {
        await Page.GotoAsync(BaseUrl);

        await Expect(Page.GetByText("Public ChirpLine")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Register account")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Login")).ToBeVisibleAsync();
    }

    /// <summary>
    /// Chirp! logotext ui is showing
    /// </summary>
    [Test]
    public async Task HomePageShowsChirpTitle ()
    {
        await Page.GotoAsync(BaseUrl);

        await Expect(Page.GetByText("Chirp!")).ToBeVisibleAsync();
    }
    
}