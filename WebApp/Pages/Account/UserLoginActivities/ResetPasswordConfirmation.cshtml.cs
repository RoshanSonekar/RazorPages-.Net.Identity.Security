using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebAppAuthentication.Pages.Account.UserLoginActivities
{
  public class ResetPasswordConfirmationModel : PageModel
  {
    public bool IsPasswordResetSuccessful { get; set; } = false;
    public void OnGet(bool IsSuccess)
    {
      IsPasswordResetSuccessful = IsSuccess;
    }
  }
}