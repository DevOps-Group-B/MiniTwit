using Microsoft.Playwright;
using NUnit.Framework;

namespace Chirp.Tests
{
/// <summary>
/// End2End class is for testing the full functionality of the Chirp application as seen by a real user.
/// It uses Playwright to automate the browser interactions in the test.
/// It tests the registration, login, logout, cheep, achievements, bio update, follow, unfollow, delete account, and other functionalities  
/// </summary>

    [TestFixture]
    public class End2End
    {
        private TestFixture<Program> _fixture = null!;
        private string email = "end2end@example.com";
        private string password = "End2end1234!";
        private string username = "End2EndUser";
        private string email2 = "end2end2@example.com";
        private string password2 = "End2end1234!";
        private string username2 = "End2EndUser2";

        private HttpClient client = null!;

        private IPlaywright playwright = null!;
        private IBrowser browser = null!;
        private IBrowserContext context = null!;
        private IPage page = null!;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _fixture = new TestFixture<Program>();
            await _fixture.EnsureServerIsReady();

            // Use the client from the fixture
            client = _fixture.Client;

            // Initialize Playwright
            playwright = await Playwright.CreateAsync();
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true // Set to false if you want to see the browser UI
            });
            context = await browser.NewContextAsync();
            page = await context.NewPageAsync();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            _fixture.StopServer();
            if (browser != null)
            {
                await browser.CloseAsync();
            }

        }

        [Test, Order(1)]
        public async Task Test_register()
        {
            // Navigate to the registration page
            var registerUrl = new Uri(client.BaseAddress!, "/Identity/Account/Register");
            await page.GotoAsync(registerUrl.ToString());

            // Fill in the registration form
            await page.FillAsync("input[name='Input.Name']", username);
            await page.FillAsync("input[name='Input.Email']", email);
            await page.FillAsync("input[name='Input.Password']", password);
            await page.FillAsync("input[name='Input.ConfirmPassword']", password);

            // Submit the registration form
            await page.ClickAsync("button[type='submit'][id='registerSubmit']");

            // Wait for navigation to complete
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify that the user is redirected to the home page
            var currentUrl = page.Url;
            NUnit.Framework.Assert.That(currentUrl, Is.EqualTo(client.BaseAddress!.ToString()));
        }

        [Test, Order(2)]
        public async Task Test_cheep()
        {
            // Add delay so register and first cheep achievement is not recieved at the same time 
            Thread.Sleep(3000);
            
            // Submit a new cheep
            var cheepMessage = "Hello, world!";
            await page.FillAsync("input[name='FormData.Message']", cheepMessage);
            await page.ClickAsync("input[type='submit']");

            // Wait for the cheep to appear
            await page.WaitForSelectorAsync($"text={cheepMessage}");

            // Verify the cheep is displayed
            var cheepText = await page.InnerTextAsync($"text={cheepMessage}");
            NUnit.Framework.Assert.That(cheepText, Does.Contain(cheepMessage));
        }

        [Test, Order(3)]
        public async Task Test_achievements()
        {
            // Check for achievements
            var myTimeLine = new Uri(client.BaseAddress!, "/" + username);
            await page.GotoAsync(myTimeLine.ToString());
            var achievements = await page.InnerTextAsync("h4:has-text('Novice Cheepster')");
            await page.WaitForSelectorAsync("h4:has-text('Novice Cheepster')");
            NUnit.Framework.Assert.That(achievements, Does.Contain("Novice Cheepster"));
        }

        [Test, Order(4)]
        public async Task Test_logout()
        {
            // Navigate to the logout page
            var index = new Uri(client.BaseAddress!, "");
            await page.GotoAsync(index.ToString());

            // Click the logout button
            await page.ClickAsync("button[type='submit']");

            // Wait for navigation to complete
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify that the user is redirected to the login page
            var currentUrl = page.Url;
            NUnit.Framework.Assert.That(currentUrl, Is.EqualTo(new Uri(client.BaseAddress!, "/Identity/Account/Logout").ToString()));

        }

        [Test, Order(5)]
        public async Task Test_login()
        {
            // Navigate to the logout page
            var loginUrl = new Uri(client.BaseAddress!, "/Identity/Account/Login");
            await page.GotoAsync(loginUrl.ToString());

            // Fill in the login form
            await page.FillAsync("input[name='Input.Email']", email);
            await page.FillAsync("input[name='Input.Password']", password);
            // Click the logout button
            await page.ClickAsync("button[type='submit'][id='login-submit']");


            // Wait for navigation to complete
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify that the user is redirected to the home page
            var currentUrl = page.Url;
            NUnit.Framework.Assert.That(currentUrl, Is.EqualTo($"{client.BaseAddress!.ToString()}{username}"));
        }

        [Test, Order(6)]
        public async Task Test_update_bio()
        {
            // Navigate to the user timeline page
            var myTimeLine = new Uri(client.BaseAddress!, "/" + username);
            await page.GotoAsync(myTimeLine.ToString());

            // Click the edit button
            await page.ClickAsync("#edit-button");

            // Fill in the new bio
            var newBio = "This is my bio.";
            await page.FillAsync("#editInput", newBio);

            // Save the new bio
            await page.ClickAsync("#save-button");

            // Wait for the bio to be updated
            await page.WaitForSelectorAsync($"text={newBio}");

            // Verify the bio is updated
            var bioText = await page.InnerTextAsync("#author-bio");
            NUnit.Framework.Assert.That(bioText, Does.Contain(newBio));
        }

        [Test, Order(7)]
        public async Task Test_cancel_bio_update()
        {
            // Navigate to the user timeline page
            var myTimeLine = new Uri(client.BaseAddress!, "/" + username);
            await page.GotoAsync(myTimeLine.ToString());

            // Click the edit button
            await page.ClickAsync("#edit-button");

            // Fill in the new bio
            var newBio = "This is my new bio.";
            await page.FillAsync("#editInput", newBio);

            // Cancel the bio update
            await page.ClickAsync("#cancel-button");


            // Verify the bio is not updated
            var bioText = await page.InnerTextAsync("#author-bio");
            NUnit.Framework.Assert.That(bioText, Does.Not.Contain(newBio));
        }

        [Test, Order(8)]
        public async Task Test_follow_author()
        {
            //Temp fix for these 2 tests, just create new user, not rely on existing DB Jackeline..
            
            // 1. Logout current user (End2EndUser)
            await page.GotoAsync(new Uri(client.BaseAddress!, "").ToString());
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // 2. Register DummyUser
            await page.GotoAsync(new Uri(client.BaseAddress!, "/Identity/Account/Register").ToString());
            await page.FillAsync("input[name='Input.Name']", "DummyUser");
            await page.FillAsync("input[name='Input.Email']", "dummy@example.com");
            await page.FillAsync("input[name='Input.Password']", "Dummy1234!");
            await page.FillAsync("input[name='Input.ConfirmPassword']", "Dummy1234!");
            await page.ClickAsync("button[type='submit'][id='registerSubmit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // 3. Post a cheep as DummyUser (This guarantees the Follow button renders!)
            await page.FillAsync("input[name='FormData.Message']", "Dummy cheep!");
            await page.ClickAsync("input[type='submit']");
            await page.WaitForSelectorAsync("text=Dummy cheep!");

            // 4. Logout DummyUser
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // 5. Log back in as original End2EndUser
            await page.GotoAsync(new Uri(client.BaseAddress!, "/Identity/Account/Login").ToString());
            await page.FillAsync("input[name='Input.Email']", email);
            await page.FillAsync("input[name='Input.Password']", password);
            await page.ClickAsync("button[type='submit'][id='login-submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // --- END SETUP ---

            // Navigate to DummyUser's timeline
            var authorTimeline = new Uri(client.BaseAddress!, "/DummyUser");
            await page.GotoAsync(authorTimeline.ToString());

            // Click the follow button
            await page.ClickAsync("button:has-text('[Follow]')");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the follow action
            var followButtonText = await page.InnerTextAsync("button:has-text('[Unfollow]')");
            NUnit.Framework.Assert.That(followButtonText, Is.EqualTo("[Unfollow]"));

            // Verify the author is in the following list
            var aboutMeUrl = new Uri(client.BaseAddress!, "/AboutMe");
            await page.GotoAsync(aboutMeUrl.ToString());
            var followingText = await page.InnerTextAsync("p:has-text('Following: 1')");
            NUnit.Framework.Assert.That(followingText, Is.EqualTo("Following: 1"));
        }

        [Test, Order(9)]
        public async Task Test_unfollow_author()
        {
            // Navigate to DummyUser's timeline
            var authorTimeline = new Uri(client.BaseAddress!, "/DummyUser");
            await page.GotoAsync(authorTimeline.ToString());

            // Click the unfollow button
            await page.ClickAsync("button:has-text('[Unfollow]')");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the unfollow action
            var followButtonText = await page.InnerTextAsync("button:has-text('[Follow]')");
            NUnit.Framework.Assert.That(followButtonText, Is.EqualTo("[Follow]"));

            // Verify the author is not in the following list
            var aboutMeUrl = new Uri(client.BaseAddress!, "/AboutMe");
            await page.GotoAsync(aboutMeUrl.ToString());
            var followingText = await page.InnerTextAsync("p:has-text('Following: 0')");
            NUnit.Framework.Assert.That(followingText, Is.EqualTo("Following: 0"));
        }

        [Test, Order(10)]
        public async Task Test_logout_register_new_user()
        {
            // Navigate to the logout page
            var index = new Uri(client.BaseAddress!, "");
            await page.GotoAsync(index.ToString());

            // Click the logout button
            await page.ClickAsync("button[type='submit']");

            // Wait for navigation to complete
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify that the user is redirected to the login page
            var currentUrl = page.Url;
            NUnit.Framework.Assert.That(currentUrl, Is.EqualTo(new Uri(client.BaseAddress!, "/Identity/Account/Logout").ToString()));

            // Navigate to the registration page
            var registerUrl = new Uri(client.BaseAddress!, "/Identity/Account/Register");
            await page.GotoAsync(registerUrl.ToString());

            // Fill in the registration form
            await page.FillAsync("input[name='Input.Name']", username2);
            await page.FillAsync("input[name='Input.Email']", email2);
            await page.FillAsync("input[name='Input.Password']", password2);
            await page.FillAsync("input[name='Input.ConfirmPassword']", password2);

            // Submit the registration form
            await page.ClickAsync("button[type='submit'][id='registerSubmit']");

            // Wait for navigation to complete
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify that the user is redirected to the home page
            currentUrl = page.Url;
            NUnit.Framework.Assert.That(currentUrl, Is.EqualTo(client.BaseAddress!.ToString()));
        }

        [Test, Order(11)]
        public async Task Test_follow_author_after_register() {

            //Navigate to the user timeline page of the author to follow
            var authorToFollow = username;
            var authorTimeline = new Uri(client.BaseAddress!, "/" + authorToFollow);
            await page.GotoAsync(authorTimeline.ToString());

            // Click the follow button
            await page.ClickAsync("button:has-text('[Follow]')");

            // Wait for the follow action to complete
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the follow action
            var followButtonText = await page.InnerTextAsync("button:has-text('[Unfollow]')");
            NUnit.Framework.Assert.That(followButtonText, Is.EqualTo("[Unfollow]"));
        }

        [Test, Order(12)]
        public async Task Test_see_other_user_cheeps_once_forllowed()
        {
            // Navigate to the user timeline 
            var authorToFollow = username2;
            var authorTimeline = new Uri(client.BaseAddress!, "/" + authorToFollow);
            await page.GotoAsync(authorTimeline.ToString());


            // Verify the cheeps of the authors are displayed
            var cheepText = await page.InnerTextAsync($"text={username}");
            NUnit.Framework.Assert.That(cheepText, Does.Contain(username));
            var cheepText2 = await page.InnerTextAsync($"text={username2}");
            NUnit.Framework.Assert.That(cheepText2, Does.Contain(username2));
        }

        [Test, Order(13)]
        public async Task Test_logout_after_register_previous_tests() {
            // Navigate to the logout page
            var index = new Uri(client.BaseAddress!, "");
            await page.GotoAsync(index.ToString());

            // Click the logout button
            await page.ClickAsync("button[type='submit']");

            // Wait for navigation to complete
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify that the user is redirected to the login page
            var currentUrl = page.Url;
            NUnit.Framework.Assert.That(currentUrl, Is.EqualTo(new Uri(client.BaseAddress!, "/Identity/Account/Logout").ToString()));
        }
    

        [Test, Order(14)]
        public async Task login_followed_user()
        {
            // Navigate to the login page
            var loginUrl = new Uri(client.BaseAddress!, "/Identity/Account/Login");
            await page.GotoAsync(loginUrl.ToString());

            // Fill in the login form
            await page.FillAsync("input[name='Input.Email']", email);
            await page.FillAsync("input[name='Input.Password']", password);
            // Click the logout button
            await page.ClickAsync("button[type='submit'][id='login-submit']");
            // Wait for navigation to complete
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the author is being followed 
            var aboutMeUrl = new Uri(client.BaseAddress!, "/AboutMe");
            await page.GotoAsync(aboutMeUrl.ToString());
            var followingText = await page.InnerTextAsync("p:has-text('Followers: 1')");
            NUnit.Framework.Assert.That(followingText, Is.EqualTo("Followers: 1"));
        }



        [Test, Order(15)]
        public async Task Test_delete_account()
        {
            // Delete the account
            await page.GotoAsync(new Uri(client.BaseAddress!, "/AboutMe").ToString());
            await page.ClickAsync("button[type='submit'][name='forgetMeBTN']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify the account is deleted
            var loginUrl = new Uri(client.BaseAddress!, "/Identity/Account/Login");
            await page.GotoAsync(loginUrl.ToString());
            await page.FillAsync("input[name='Input.Email']", email);
            await page.FillAsync("input[name='Input.Password']", password);
            await page.ClickAsync("button[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var errorMessage = await page.InnerTextAsync(".validation-summary-errors");
            NUnit.Framework.Assert.That(errorMessage, Does.Contain("Invalid login attempt."));
        }

    }
}
