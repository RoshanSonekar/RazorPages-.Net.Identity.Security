using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebAppRazor_Security.Pages.Settings
{
  [Authorize(Policy ="AdminOnly")]
    public class SettingModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
