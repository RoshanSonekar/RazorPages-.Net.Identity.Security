using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Protocols;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using WebApp.Data.Account;

namespace WebApp.Pages.Account;

public class LoginModel : PageModel
{
	// gmail Roshan@2026

	private readonly SignInManager<User> signInManager;
	private readonly IConfiguration config;
	public LoginModel(SignInManager<User> _signInManager, IConfiguration _config)
	{
		signInManager = _signInManager;
		config = _config;
	}
	public string TwoFAAuthenticationType { get; private set; } = "None"; // "Email" or "AuthenticatorApp"

	[BindProperty]
	public CredentialViewModel Credential { get; set; } = new CredentialViewModel();
	[BindProperty]
	public string ReturnUrl { get; set; } = string.Empty;

	public void OnGet()
	{
	}

	public async Task<IActionResult> OnPostAsync()
	{
		if (!ModelState.IsValid) return Page();

			 var result = await signInManager.PasswordSignInAsync(
				Credential.Email,
				Credential.Password,
				isPersistent: Credential.RememberMe,
				false);

		TwoFAAuthenticationType = config["2FAAuthenticationType"] ?? "Email";
		if (result.Succeeded)
		{
			bool mfaRequired = bool.Parse(config["MFARequired"]?? "false");
			if (mfaRequired && TwoFAAuthenticationType == "AuthenticatorApp")
			{                                         
				var loggedInuser = await signInManager.UserManager.FindByEmailAsync(Credential.Email);
				if (loggedInuser is not null)
				{
					// setup authenticator app for the first time               
					var getTokenResult = await signInManager.UserManager.GetAuthenticatorKeyAsync(loggedInuser);
					if (getTokenResult is null) // setup authenticator app for the first time
						return RedirectToPage("/Account/AuthenticatorWithMFASetup");

					var isTwoFactorEnabled = await signInManager.UserManager.GetTwoFactorEnabledAsync(loggedInuser);
					if (!isTwoFactorEnabled)
						return RedirectToPage("/Account/AuthenticatorWithMFASetup");
				}
			}
			return RedirectToPage("/Index");
		}
		else
		{
			if (result.RequiresTwoFactor)
			{
				var loggedInuser = await signInManager.UserManager.FindByEmailAsync(Credential.Email);
				if (loggedInuser is not null)
					TwoFAAuthenticationType = loggedInuser.AuthenticationType ?? "Email";
				
				// for email security code, redirect to TwoFactorLogin page
				if (TwoFAAuthenticationType == "Email")
				{
					return RedirectToPage("/Account/TwoFactorLogin",
						new
						{
							Credential.Email,
							Credential.RememberMe
						});
				}
				// for authenticator app security code, redirect to TwoFactorLoginWithAuthenticatorApp page
				else if (TwoFAAuthenticationType == "AuthenticatorApp")
				{
					return RedirectToPage("/Account/TwoFactorLoginWithAuthenticatorApp",
					new
					{
						Credential.RememberMe
					});
				}
				else
				{
					ModelState.AddModelError("Login", "'RequiresTwoFactor' is enableed for the user but 2FA authentication type is not defined.");
					return Page();
				}
			}

			if (result.IsLockedOut)
				ModelState.AddModelError("Login", "You are locked out.");
			else
				ModelState.AddModelError("InvalidCredentials", "Invalid username or password. Please try again.");

			return Page();
		}
	}
}
   
public class CredentialViewModel
{
	[Required]
	[Display(Name = "Password")]
	[DataType(DataType.Password)]
	public string Password { get; set; } = string.Empty;

	[Required]
	[Display(Name = "Email")]
	[EmailAddress(ErrorMessage = "Invalid email address.")]
	public string Email { get; set; } = string.Empty;

	[Display(Name = "Remember Me?")]
	public bool RememberMe { get; set; } = false;
	public bool IsTwoFactorEnabled { get; set; } = false;
}