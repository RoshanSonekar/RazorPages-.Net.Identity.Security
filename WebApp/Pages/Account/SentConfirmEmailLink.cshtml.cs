using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebAppAuthentication.Pages.Account
{
  public class SentConfirmEmailLinkModel : PageModel
  {
    public string Message { get; set; } = "Confirmation link sent to your email successfully! Kindly check your inbox or spam then click the link to verify your email address.";
    public void OnGet()
    {
    }
  }
}
