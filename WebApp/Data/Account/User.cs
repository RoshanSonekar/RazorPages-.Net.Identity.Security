using Microsoft.AspNetCore.Identity;

namespace WebApp.Data.Account
{
	public class User:IdentityUser
	{
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public DateTime DateOfBirth { get; set; } 
		public string Department { get; set; } = string.Empty;	
		public string Designation { get; set; } = string.Empty;
		public string AuthenticationType { get; set; } = "Email"; // "Email" or "AuthenticatorApp"
		public byte[]? ProfilePicture { get; set; }
		public string MobileNumber { get; set; } = string.Empty;
	}
}
