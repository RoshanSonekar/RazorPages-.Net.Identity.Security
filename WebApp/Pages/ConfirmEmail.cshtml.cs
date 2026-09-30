using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
	public class ConfirmEmailModel : PageModel
	{

		private readonly UserManager<IdentityUser> userManager;
		public string Message { get; set; } = string.Empty;
		public ConfirmEmailModel(UserManager<IdentityUser> _userManager)
		{
			userManager = _userManager;
		}
		//Sept#2026@
		//admin@roshansonekar.com
//		SMTP server smtp - relay.brevo.com
//Port	587
//Login bbad67001@smtp - brevo.com
//Password Open SMTP key settings

		public async Task<IActionResult> OnGet(string userId, string token)
		{
			var user = await userManager.FindByIdAsync(userId);
			if (user is null)
			{
				Message = "Failed to validate email.";
				return Page();
			}

			var result = await userManager.ConfirmEmailAsync(user, token);
			if (result.Succeeded)
				Message = "Email address is confirmed. You may try to login.";
			
			return Page();
		}
	}
}
