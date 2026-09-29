using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace WebApp.Pages.Account;

public class LoginModel : PageModel
{
	private readonly SignInManager<IdentityUser> signInManager;
	public LoginModel(SignInManager<IdentityUser> _signInManager)
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