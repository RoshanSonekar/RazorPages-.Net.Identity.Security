using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using WebAppRazor_Security.Authorisation;

namespace WebAppRazor_Security.Pages.Account
{
  public class LoginModel : PageModel
  {
    [BindProperty]
    public Credential Credential { get; set; } = new Credential();
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost() 
    {
      if (!ModelState.IsValid)
        return Page();

      // verify credentials
      if (Credential.Username == "admin" && Credential.Password == "password")
      {
        // create security context
        var claims = new List<Claim>
        {
          new Claim(ClaimTypes.Name, "admin"),
          new Claim(ClaimTypes.Email, "admin@gmail.com"),
          new Claim("Admin","true"),
					new Claim("Manager","true"),
					new Claim("Department","HR"), // custom claim for authorisation policy >> to get from DB
					new Claim("EmploymentDate","2025-09-09"),

				};

        var identity = new ClaimsIdentity(claims, "MyCookieAuth");
        ClaimsPrincipal claimsPrincipal =  new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
          IsPersistent = Credential.RememberMe
        };

        await HttpContext.SignInAsync("MyCookieAuth", claimsPrincipal, authProperties);

        return RedirectToPage("/Index");
      }

      return Page(); 
    }
  }
}
