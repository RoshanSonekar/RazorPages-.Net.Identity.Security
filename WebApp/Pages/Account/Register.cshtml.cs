using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Account;
using WebApp.Integration.Email;

namespace WebApp.Pages.Account
{
  public class RegisterModel : PageModel
	{
		private readonly IEmailService emailService;
		private readonly UserManager<User> userManager;

		public RegisterModel(UserManager<User> _userManager, IEmailService _emailService)
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
      var user = new User
			{
        Email = registerViewModel.Email,
        UserName = registerViewModel.Email,
				Department = registerViewModel.Department,
				Designation = registerViewModel.Designation
			};

			// create claims
			List<System.Security.Claims.Claim> claimsList = new List<System.Security.Claims.Claim>();
			claimsList.Add(new System.Security.Claims.Claim("Department", registerViewModel.Department));
			claimsList.Add(new System.Security.Claims.Claim("Designation", registerViewModel.Designation));


			var result =  await userManager.CreateAsync(user, registerViewModel.Password);

      if (result.Succeeded)
      {
				await userManager.AddClaimsAsync(user, claimsList); 

				// generate token and link
				var emailConfirmationToken =  await userManager.GenerateEmailConfirmationTokenAsync(user);
				var confirmationLink = Url.PageLink(pageName: "/Account/ConfirmEmail", values: new { userId = user.Id, token = emailConfirmationToken });

				// send token via email
				//await emailService.Send(new EmailConfirmationModelRequest() { Email=user.Email, Subject="Verify Email", MessageBody= $"Please click on this link to confirm your email - { confirmationLink }" });

        return RedirectToPage("/Account/login");
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

		[Required]
		public string Department { get; set; } = string.Empty;

		[Required]
		public string Designation { get; set; } = string.Empty;
	} 
}
