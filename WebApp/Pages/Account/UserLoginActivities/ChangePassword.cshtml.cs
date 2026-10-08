using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Account;

namespace WebAppAuthentication.Pages.Account.UserLoginActivities
{
  [Authorize]
  public class ChangePasswordModel : PageModel
  {
		[BindProperty]
		public ChangePasswordViewModel changePasswordViewModel { get; set; }

		[BindProperty]
		public string Email { get; set; } = string.Empty;

		private readonly SignInManager<User> signInManager;
		public ChangePasswordModel(SignInManager<User> _signInManager)
		{
			signInManager = _signInManager;
			changePasswordViewModel = new ChangePasswordViewModel();	
		}

		public void OnGet()
    {
			// If someone navigates directly via GET, we pre-fill the email from the logged-in user context
			if (User.Identity?.IsAuthenticated == true)
			{
				Email = User.Identity.Name ?? string.Empty;
			}
		}

		public async Task<IActionResult> OnPostAsync()
		{ 
			if (!ModelState.IsValid) return Page();

			bool success = false;
			var user = await signInManager.UserManager.FindByEmailAsync(Email);
			if (user is null)
			{
				ModelState.AddModelError(string.Empty, "User not found.");
				return Page();
			}

			var result = await signInManager.UserManager.ChangePasswordAsync(user, changePasswordViewModel.CurrentPassword, changePasswordViewModel.NewPassword);
			if (result.Succeeded)
			{
				await signInManager.RefreshSignInAsync(user);
				await signInManager.SignOutAsync(); // Sign out the user after password change
				success = true;
				return RedirectToPage("/Account/UserLoginActivities/ChangePasswordConfirmation" ,new { isSuccess = success });
			}
			else
			{
				foreach (var error in result.Errors)
					ModelState.AddModelError(string.Empty, error.Description);
				
				return Page();
			}
		}

	}

  public class ChangePasswordViewModel
	{ 
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Current Password")]
		public string CurrentPassword { get; set; } = string.Empty;
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "New Password")]
		public string NewPassword { get; set; } = string.Empty;
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm New Password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmNewPassword { get; set; } = string.Empty;
	}
}
