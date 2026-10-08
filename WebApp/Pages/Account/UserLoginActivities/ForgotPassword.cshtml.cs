using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Account;
using WebApp.Integration.Email;
using Microsoft.AspNetCore.DataProtection;

namespace WebAppAuthentication.Pages.Account.UserLoginActivities
{
	public class ForgotPasswordModel : PageModel
	{
		[BindProperty]
		public ForgotPasswordViewModel forgotPasswordViewModel { get; set; }

		private readonly IDataProtector protector;
		private readonly UserManager<User> userManager;
		private readonly IEmailService emailService;

		public ForgotPasswordModel(UserManager<User> _userManager, IEmailService _emailService, IDataProtectionProvider _protector)
		{
			forgotPasswordViewModel = new ForgotPasswordViewModel();
			userManager = _userManager;
			emailService = _emailService;
			// Use a unique purpose string specific to password resets
			protector = _protector.CreateProtector("PasswordReset.Purpose.v1");
		}

		public void OnGet()
		{
		}

		private string EncryptUserId(string userId)
		{
			return protector.Protect(userId);
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid) return Page();

			var user = await userManager.FindByEmailAsync(forgotPasswordViewModel.Email);

			// user not exist or email not confirmed
			// Don't reveal that the user does not exist or is not confirmed (standard practice)
			if (user is null || !(await userManager.IsEmailConfirmedAsync(user)))
				return RedirectToPage("/Account/UserLoginActivities/ForgotPasswordConfirmation");

			forgotPasswordViewModel.Email = user.Email??string.Empty;

			// generate password reset token
			var newToken = await userManager.GeneratePasswordResetTokenAsync(user);

			// encode the token to make it URL safe
			//var encodedToken = System.Web.HttpUtility.UrlEncode(token);
			var encodedToken = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(newToken));
			var encryptedUserId = EncryptUserId(user.Id); // encrypt the user ID for security in the URL

			var forgotPasswordConfirmationLink = Url.PageLink(pageName: "/Account/UserLoginActivities/ResetPassword", values: new { userId = encryptedUserId, token = encodedToken });

			// send token via email
			await emailService.Send(new EmailConfirmationModelRequest() { Email = forgotPasswordViewModel.Email, Subject = "Password Reset Link", MessageBody = $"Please click on this link to reset your password - {forgotPasswordConfirmationLink}" });

			return RedirectToPage("/Account/UserLoginActivities/ForgotPasswordConfirmation");
		}
	}

  public class ForgotPasswordViewModel
  {
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;
	}
}
