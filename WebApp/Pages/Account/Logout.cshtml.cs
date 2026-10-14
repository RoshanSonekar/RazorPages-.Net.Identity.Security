using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
  public class LogoutModel : PageModel
  {
    private readonly SignInManager<IdentityUserExtension> signInManager; 
		public LogoutModel(SignInManager<IdentityUserExtension> _signInManager)
		{
			signInManager = _signInManager;
		}
		public async Task<IActionResult> OnPostAsync()
    {
      await signInManager.SignOutAsync();
     
      return RedirectToPage("/Account/Login");
    }
  }
}
