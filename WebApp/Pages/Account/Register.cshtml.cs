using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Integration.Email;

namespace WebApp.Pages.Account
{
  public class RegisterModel : PageModel
	{
		private readonly IEmailService emailService;
		private readonly UserManager<IdentityUser> userManager;

		public RegisterModel(UserManager<IdentityUser> _userManager, IEmailService _emailService)
		{
			userManager = _userManager;
			emailService = _emailService;
		}

		[BindProperty]
    public RegisterViewModel registerViewModel { get; set; } = new RegisterViewModel();

    public void OnGet()
    {
		}

    public async Task<IActionResult> OnPostAsync()
    {
      if(!ModelState.IsValid) return Page();

      // create user
      var user = new IdentityUser
      {
        Email = registerViewModel.Email,
        UserName = registerViewModel.Email
      };
      var result =  await userManager.CreateAsync(user, registerViewModel.Password);

      if (result.Succeeded)
      {
        // generate token
        var emailConfirmationToken =  await userManager.GenerateEmailConfirmationTokenAsync(user);

        // send token via email
        await emailService.Send(new EmailConfirmationModelRequest() { Email="roshansonekar@gmail.com", Subject="Testing API", MessageBody= emailConfirmationToken });

				return Redirect(Url.PageLink(pageName: "/Account/ConfirmEmail", values: new { userId = user.Id, token = emailConfirmationToken }) ?? string.Empty);
			}
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
