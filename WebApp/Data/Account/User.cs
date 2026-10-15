using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace WebApp.Data.Account
{
	public class UserProfile //: IdentityUser
	{
		public Guid Id { get; set; }  
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;

		[JsonPropertyName("DateOfBirth")]
		public DateTime DateOfBirth { get; set; }  
		bool Active { get; set; }
		public string MobileNumber { get; set; } = string.Empty;
		//public string Payload { get; set; } = string.Empty;
	}

	public class IdentityUserExtension : IdentityUser
	{
		public string AuthenticationType { get; set; } = "Email"; // "Email" or "AuthenticatorApp"
		public byte[]? ProfilePicture { get; set; } 
		public bool IsProfileExist { get; set; } = false;
	}
}
