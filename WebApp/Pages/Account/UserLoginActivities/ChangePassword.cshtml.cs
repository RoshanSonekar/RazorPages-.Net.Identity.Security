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


		private readonly SignInManager<User> signInManager;
		public ChangePasswordModel(SignInManager<User> _signInManager)
		{
			signInManager = _signInManager;
			changePasswordViewModel = new ChangePasswordViewModel();	
		}

		public void OnGet(string Email)
    {
      changePasswordViewModel.Email = Email;
    }

		public async Task<IActionResult> OnPostAsync(string Email)
		{
			changePasswordViewModel.Email = Email;

			if (!ModelState.IsValid) return Page();

			bool success = false;
			var user = await signInManager.UserManager.FindByEmailAsync(changePasswordViewModel.Email);
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
		public string Email { get; set; } = string.Empty;

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
