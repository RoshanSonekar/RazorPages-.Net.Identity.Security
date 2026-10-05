using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using WebApp.Data.Account;
using WebApp.Integration.Email;

namespace WebApp.Pages.Account;

public class LoginModel : PageModel
{
	private readonly SignInManager<User> signInManager;
	public LoginModel(SignInManager<User> _signInManager)
	{
		signInManager = _signInManager;
	}

	[BindProperty]
	public CredentialViewModel Credential { get; set; } = new CredentialViewModel();
	public void OnGet()
	{
	}

	public async Task<IActionResult> OnPost()
	{
		if (!ModelState.IsValid) return Page();

		var result = await signInManager.PasswordSignInAsync(
			Credential.Email,
			Credential.Password,
			Credential.RememberMe,
			false);

		if (result.Succeeded)
			return RedirectToPage("/Index");
		else
		{
			if (result.RequiresTwoFactor) // setup type in IdentityOptions.SignIn.TwoFactorProvider = "Email" or "Authenticator"
			{
				// for email security code, redirect to TwoFactorLogin page
				// return RedirectToPage("/Account/TwoFactorLogin", new { Credential.Email, Credential.RememberMe });

				// for authenticator app security code, redirect to TwoFactorLoginWithAuthenticatorApp page
				return RedirectToPage("/Account/TwoFactorLoginWithAuthenticatorApp", 
					new 
					{ 
						// Credential.Email, 
						Credential.RememberMe 
					});
			}

			if (result.IsLockedOut)
				ModelState.AddModelError("Login", "You are locked out.");
			else
				ModelState.AddModelError("Login", "Failed to login.");

			return Page();
		}

	}
}
   
public class CredentialViewModel
{
	[Required]
	[Display(Name = "Password")]
	public string Password { get; set; } = string.Empty;

	[Required]
	[Display(Name = "Email")]
	public string Email { get; set; } = string.Empty;

	[Display(Name = "Remember Me?")]
	public bool RememberMe { get; set; } = false;
}