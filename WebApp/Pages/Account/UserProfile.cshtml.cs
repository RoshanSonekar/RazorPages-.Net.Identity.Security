using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
  [Authorize]
  public class UserProfileModel : PageModel
  {
    private readonly UserManager<User> userManager;

    [BindProperty]
    public UserProfileViewModel UserProfileView { get; set; }
    [BindProperty]
    public string? SuccessMessage{ get; set; }
    [BindProperty]
    public string Email { get; set; } = string.Empty;

		public UserProfileModel(UserManager<User> _userManager)
    {
      userManager = _userManager;
			UserProfileView = new UserProfileViewModel();
		}

    public async Task<IActionResult> OnGetAsync()
    {
      SuccessMessage = string.Empty;
      // If someone navigates directly via GET, we pre-fill the email from the logged-in user context
      if (User.Identity?.IsAuthenticated == true)
      {
        Email = User.Identity.Name ?? string.Empty;
        var (user, departmentClaim, designationClaim) = await GetUserInfoAsync(Email);

        if (user is not null)
        {
          UserProfileView.Department = departmentClaim?.Value ?? string.Empty;
          UserProfileView.Designation = designationClaim?.Value ?? string.Empty;
        }
      }
      return Page();
		}

    public async Task<IActionResult> OnPostAsync()
    {
      if (!ModelState.IsValid)
        return Page();

      try
      {
				var (user, departmentClaim, designationClaim) = await GetUserInfoAsync(Email);
				if (user is null)
					return NotFound();

				if (departmentClaim is not null)
					await userManager.ReplaceClaimAsync(user, departmentClaim, new Claim(departmentClaim.Type, UserProfileView.Department ?? string.Empty));

				if (designationClaim is not null)
					await userManager.ReplaceClaimAsync(user, designationClaim, new Claim(designationClaim.Type, UserProfileView.Designation ?? string.Empty));
			}
      catch (Exception ex)
      {
        ModelState.AddModelError("User Profile", "An error occurred while updating the user profile.");
      }
      SuccessMessage = "User profile updated successfully.";
			return Page();
    }

    private async Task<(User? user, Claim? departmentClaim, Claim? designationClaim)> GetUserInfoAsync(string email)
    {
			var user = await userManager.FindByEmailAsync(email);
			if (user is not null)
			{
				var claims = await userManager.GetClaimsAsync(user);

        var departmentClaim = claims.FirstOrDefault(c => c.Type == "Department");
        var designationClaim = claims.FirstOrDefault(c => c.Type == "Designation");

				return (user, departmentClaim, designationClaim);
			}
			return (null, null, null);
		}
  }

  public class UserProfileViewModel
	{
    [Required]
		public string? Department { get; set; }

    [Required]
    public string? Designation { get; set; }
	}
}
