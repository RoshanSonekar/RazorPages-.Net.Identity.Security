using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using WebApp.Data.Account;

namespace WebAppAuthentication.Pages.Account.UserLoginActivities
{
	public class ResetPasswordModel : PageModel
	{
		[BindProperty]
		public ResetPasswordViewModel resetPasswordViewModel { get; set; }

		[BindProperty]
		public bool isSuccess { get; set; } = false;

		// Matches ?userId= from your URL
		[BindProperty(SupportsGet = true)]
		public string UserId { get; set; } = string.Empty;

		// Matches ?token= from your URL
		[BindProperty(SupportsGet = true)]
		public string Token { get; set; } = string.Empty;

		private readonly UserManager<User> userManager;
		private readonly IDataProtector protector;

		public ResetPasswordModel(UserManager<User> _userManager, IDataProtectionProvider _protector)
		{
			userManager = _userManager;
			resetPasswordViewModel = new ResetPasswordViewModel();

			// Use a unique purpose string specific to password resets
			protector = _protector.CreateProtector("PasswordReset.Purpose.v1");
		}

		private string DecryptUserId(string encryptedValue)
		{
			return protector.Unprotect(encryptedValue);
		}


		public IActionResult OnGet()
		{
			// If the link is malformed and missing the token or email, return an error or redirect
			if (string.IsNullOrEmpty(UserId) || string.IsNullOrEmpty(Token)) 
				return BadRequest("A code and email must be supplied for password reset.");

			resetPasswordViewModel.UserId = DecryptUserId(UserId);
			resetPasswordViewModel.Token = Token; 

			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid) return Page();

			// Here you would typically call your user manager to reset the password using the provided token.
			// For example:
			var user = await userManager.FindByIdAsync(resetPasswordViewModel.UserId);

			if (user is null) 
			{ 
				ModelState.AddModelError("Reset Password", "Invalid request."); 
				return Page();
			}
			var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPasswordViewModel.Token));
			var result = await userManager.ResetPasswordAsync(user, decodedToken, resetPasswordViewModel.Password);
			if (result.Succeeded)
			{ 
				isSuccess = true;
				await userManager.UpdateSecurityStampAsync(user);
			}

			return RedirectToPage("/Account/UserLoginActivities/ResetPasswordConfirmation", new { IsSuccess = isSuccess });
		}
	}

	public class ResetPasswordViewModel
	{
		[Required(ErrorMessage = "UserId is required.")] 
		public string UserId { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password is required.")]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; } = string.Empty;

		[Required(ErrorMessage = "Confirm Password is required.")]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm Password")]
		[Compare(nameof(Password), ErrorMessage = "The password and confirmation password must match.")]
		public string ConfirmPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "Token is required.")]
		public string Token { get; set; } = string.Empty;
	} 
}
