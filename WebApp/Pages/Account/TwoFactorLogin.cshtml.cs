using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Account;
using WebApp.Integration.Email;

namespace WebApp.Pages.Account
{
	public class TwoFactorLoginModel : PageModel
	{
		[BindProperty]
		public TwoFactorLoginViewModel TwoFactorLoginViewModel { get; set; }

		private readonly UserManager<User> userManager;
		private readonly SignInManager<User> signInManager;
		private readonly IEmailService emailService;
		public TwoFactorLoginModel(SignInManager<User>  _signInManager, UserManager<User>  _userManager, IEmailService _emailService)
		{
			signInManager = _signInManager;
			userManager = _userManager;
			emailService = _emailService;
			TwoFactorLoginViewModel = new TwoFactorLoginViewModel();
		}

		public async Task OnGetAsync(string email, bool rememberMe)
		{
			var user = await userManager.FindByEmailAsync(email);
			TwoFactorLoginViewModel.SecurityCode = string.Empty;
			TwoFactorLoginViewModel.RememberMe = rememberMe;
			TwoFactorLoginViewModel.Email = email;
			if (user is not null)
			{
				var securityCode = await userManager.GenerateTwoFactorTokenAsync(user, "Email");
				await emailService.Send(new EmailConfirmationModelRequest()
				{
					Email = user.Email,
					Subject = "Two Factor Authentication Code",
					MessageBody = $"Your two factor authentication code is: {securityCode}"
				});
			}
		}

		public async Task<IActionResult> OnPostAsync(string email)
		{
			if (!ModelState.IsValid)
				return Page();

			var result = await signInManager.TwoFactorSignInAsync("Email", TwoFactorLoginViewModel.SecurityCode, TwoFactorLoginViewModel.RememberMe, false);
			if(result.Succeeded)
			{
				return RedirectToPage("/Index");
			}
			else
			{
				if(result.IsLockedOut)
					ModelState.AddModelError("Login2FA", "User account is locked out.");
				else
					ModelState.AddModelError("Login2FA", "Falied! Invalid security code.");
				
				return Page();
			}
		}
	}

	public class TwoFactorLoginViewModel
	{
		public string Email { get; set; } = string.Empty;
		[Required]
		[Display(Name = "Security Code")]
		public string SecurityCode { get; set; } = string.Empty;
		public bool RememberMe { get; set; }
	}
}
