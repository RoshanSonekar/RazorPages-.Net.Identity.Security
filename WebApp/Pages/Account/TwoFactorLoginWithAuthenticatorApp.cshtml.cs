using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
	public class TwoFactorLoginWithAuthenticatorAppModel : PageModel
	{
		[BindProperty]
		public TwoFactorLoginMFAViewModel LoginMFAViewModel { get; set; }
		public readonly SignInManager<User> signInManager;

		public TwoFactorLoginWithAuthenticatorAppModel(SignInManager<User> _signInManager)
		{
			signInManager = _signInManager;
			LoginMFAViewModel = new TwoFactorLoginMFAViewModel();
		}

		public void OnGet(bool rememberMe)
		{
			LoginMFAViewModel.RememberMe = rememberMe;
			LoginMFAViewModel.SecurityCode = string.Empty;
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
				return Page();

			var result = await signInManager.TwoFactorSignInAsync("Authenticator", LoginMFAViewModel.SecurityCode, LoginMFAViewModel.RememberMe, false);
			if (result.Succeeded)
			{
				return RedirectToPage("/Index");
			}
			else
			{
				if (result.IsLockedOut)
					ModelState.AddModelError("Authenticator2FA", "User account is locked out.");
				else
					ModelState.AddModelError("Authenticator2FA", "Falied! Invalid security code.");

				return Page();
			}
		}
	}

	public class TwoFactorLoginMFAViewModel
	{
		[Required]
		[Display(Name = "Security Code")]
		public string SecurityCode { get; set; } = string.Empty;
		public bool RememberMe { get; set; }
	}
}
