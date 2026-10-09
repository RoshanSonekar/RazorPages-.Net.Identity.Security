using Microsoft.AspNetCore.Identity;

namespace WebApp.Data.Account
{
	public class User:IdentityUser
	{
		public string Department { get; set; } = string.Empty;	
		public string Designation { get; set; } = string.Empty;
		public byte[]? ProfilePicture { get; set; }
	}
}
