// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Chirp.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.web.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; }

            public IFormFile ProfilePicture { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Input = new InputModel
            {
                Username = userName,
                PhoneNumber = phoneNumber,
                ProfilePicture = user.ProfilePicture != null ? new FormFile(
                    new MemoryStream(user.ProfilePicture),
                    0,
                    user.ProfilePicture.Length,
                    "ProfilePicture",
                    "profile.jpg") : null
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }
            var currentUserName = await _userManager.GetUserNameAsync(user);
            if (!string.Equals(Input.Username, currentUserName, StringComparison.Ordinal))
            {
                var setResult = await _userManager.SetUserNameAsync(user, Input.Username!);
                if (!setResult.Succeeded)
                {
                    foreach (var e in setResult.Errors)
                        ModelState.AddModelError(string.Empty, e.Description);
                    await LoadAsync(user);
                    return Page();
                }
            }
            //Check that the image uploaded is of type .png or .jpeg
            if (Input.ProfilePicture.ContentType != "image/png" && Input.ProfilePicture.ContentType != "image/jpeg")
            {
                {
                    ModelState.AddModelError("Input.ProfilePicture", "Only PNG and JPEG images are allowed.");
                    return Page();
                }
            }           
            //Check that the image size is a maximum of 2mb
            if (Input.ProfilePicture.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError("Input.ProfilePicture", "The file size must not exceed 2 MB.");
                return Page();
            }
            if (Input.ProfilePicture != null)
            {
                using var memoryStream = new MemoryStream();
                await Input.ProfilePicture.CopyToAsync(memoryStream);

                var profilePictureBytes = memoryStream.ToArray();

                if (user != null)
                {
                    user.ProfilePicture = profilePictureBytes;
                    await _userManager.UpdateAsync(user);
                }
            }
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
