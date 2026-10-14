using Microsoft.AspNetCore.Identity;

namespace WebApp.Data.Account
{
	public class UserProfile //: IdentityUser
	{
		public Guid Id { get; set; }  
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public DateTime DateOfBirth { get; set; }  
		public string MobileNumber { get; set; } = string.Empty;
	}

	public class IdentityUserExtension : IdentityUser
	{
		public string AuthenticationType { get; set; } = "Email"; // "Email" or "AuthenticatorApp"
		public byte[]? ProfilePicture { get; set; } 
		public bool IsProfileExist { get; set; } = false;
	}
}
