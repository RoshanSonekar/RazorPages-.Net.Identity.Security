using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebAppAuthentication.Pages.Account.UserLoginActivities
{
  public class ChangePasswordConfirmationModel : PageModel
  {

    [BindProperty(SupportsGet = true)]
    public bool IsSuccess { get; set; }

    public void OnGet(bool isSuccess)
    {
      this.IsSuccess = isSuccess;
    }
  }
}
