using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Account;
using WebApp.Integration.Email;

namespace WebAppAuthentication.Pages.Account
{
	public class SingUpModel : PageModel
	{
		[BindProperty]
		public SignUpViewModel signUpViewModel { get; set; } = new SignUpViewModel();

		private readonly IEmailService emailService;
		private readonly UserManager<IdentityUserExtension> userManager;

		public SingUpModel(UserManager<IdentityUserExtension> _userManager, IEmailService _emailService)
		{
			userManager = _userManager;
			emailService = _emailService;
		}

		public void OnGet()
		{

		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid) return Page();

			IdentityUserExtension identityUser = new IdentityUserExtension()
			{
				Email = signUpViewModel.Email,
				UserName = signUpViewModel.Email
			};
			var result = await userManager.CreateAsync(identityUser, signUpViewModel.Password);

			if (result.Succeeded)
			{
				// Generate token and link
				var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(identityUser);
				var confirmationLink = Url.PageLink(pageName: "/Account/ConfirmEmail", values: new { userId = identityUser.Id, token = emailConfirmationToken });

				// Send token via email
				await emailService.Send(new EmailConfirmationModelRequest()
				{
					Email = identityUser.Email,
					Subject = "Email Verification",
					MessageBody = $"Please click on this link to confirm your email - {confirmationLink}"
				});

				return RedirectToPage("/Account/SentConfirmEmailLink");
			}
			else
			{
				foreach (var error in result.Errors)
					ModelState.AddModelError("Sign-Up", error.Description);

				return Page();
			}
		}
	} 

  public class SignUpViewModel
	{
		[Required]
		[EmailAddress(ErrorMessage = "Invalid email address.")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password is required.")]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; } = string.Empty;

		[Required(ErrorMessage = "Confirm Password is required.")]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm Password")]
		[Compare(nameof(Password), ErrorMessage = "The password and confirmation password must match.")]
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}
