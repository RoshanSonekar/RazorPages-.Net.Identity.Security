using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Pages.Account
{
  public class RegisterModel : PageModel
  {
    private readonly UserManager<IdentityUser> userManager;
		public RegisterModel(UserManager<IdentityUser> _userManager)
		{
			userManager = _userManager;
		}

		[BindProperty]
    public RegisterViewModel registerViewModel { get; set; } = new RegisterViewModel();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if(!ModelState.IsValid) return Page();

      // validate email (otional)
      // create user
      var user = new IdentityUser
      {
        Email = registerViewModel.Email,
        UserName = registerViewModel.Email
      };

      var result =  await userManager.CreateAsync(user, registerViewModel.Password);
      if (result.Succeeded)
        return RedirectToPage("/Account/Login");
      else
      {
        foreach (var error in result.Errors)
          ModelState.AddModelError("Sign-Up", error.Description);

        return Page();
      }
		}
  }
  public class RegisterViewModel
  {
    [Required]
    [EmailAddress(ErrorMessage ="Invalid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(dataType:DataType.Password)]
    public string Password { get; set; } = string.Empty;
  } 
}
