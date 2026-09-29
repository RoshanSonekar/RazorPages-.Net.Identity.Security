using System.ComponentModel.DataAnnotations;

namespace WebAppRazor_Security.Authorisation
{
	public class Credential
	{
		[Required]
		[Display(Name = "Password")]
		public string Password { get; set; } = string.Empty;

		[Required]
		[Display(Name = "User Name")]
		public string Username { get; set; } = string.Empty;

		[Display(Name = "Remember Me?")]
		public bool RememberMe { get; set; } = false;
	}
}
