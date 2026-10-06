using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
  [Authorize]
  public class AuthenticatorWithMFASetupModel : PageModel
  {
    [BindProperty]
    public AuthenticatorWithMFASetupViewModel ViewModel { get; set; } = new();

		[BindProperty]
		public bool IsMFAEnabled { get; set; } = false;

		private readonly UserManager<User> userManager;
    public AuthenticatorWithMFASetupModel(UserManager<User> _userManager)
    {
      userManager = _userManager;
			IsMFAEnabled = false;
			ViewModel =new AuthenticatorWithMFASetupViewModel();
		}

    public async Task OnGetAsync()
    {
      var user = await userManager.GetUserAsync(base.User);
      if (user is not null)
      {
				await userManager.ResetAuthenticatorKeyAsync(user);

				var key = await userManager.GetAuthenticatorKeyAsync(user);
        ViewModel.AuthenticatorKey = key??string.Empty;
				ViewModel.QRCodeBytes = GenerateQRCode("MyApp", ViewModel.AuthenticatorKey, user.Email??string.Empty);
			}
		}

    public async Task<IActionResult> OnPostAsync()
    {
      if(!ModelState.IsValid)
				return Page();

			var user = await userManager.GetUserAsync(base.User);
			if (user is not null)
			{
				var isValid = await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, ViewModel.SecurityCode);
				if (isValid)
				{
					await userManager.SetTwoFactorEnabledAsync(user, true);
					IsMFAEnabled=true;
					return RedirectToPage("/Index");
				}
				else
				{
					ModelState.AddModelError("Authenticator Setup", "Something went wrong with the authenticator setup.");
					return Page();
				}
			}
			else
			{
				ModelState.AddModelError("User", "User not found.");
				return Page();
			}
    }

		private Byte[] GenerateQRCode(string provider, string authenticatorKey, string email)
		{
			var qrCodeData = $"otpauth://totp/{provider}:{email}?secret={authenticatorKey}&issuer={provider}";
			using (var qrGenerator = new QRCoder.QRCodeGenerator())
			{
				var qrCode = qrGenerator.CreateQrCode(qrCodeData, QRCoder.QRCodeGenerator.ECCLevel.Q);
				using (var qrCodeImage = new QRCoder.PngByteQRCode(qrCode))
				{
					return qrCodeImage.GetGraphic(20);
				}
			}
		}
	}

	public class AuthenticatorWithMFASetupViewModel
	{
		public string? AuthenticatorKey { get; set; }

    [Required]
    [Display(Name = "Security Code")]
    public string SecurityCode { get; set; } = string.Empty;

		public Byte[] QRCodeBytes { get; set; }
	}
}
