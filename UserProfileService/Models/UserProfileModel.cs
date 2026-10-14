using System.ComponentModel.DataAnnotations;

namespace UserProfileService.Models
{
	public class UserProfileModel
	{
		[Required]
		public Guid Id { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public DateTime DateOfBirth { get; set; } 
		public bool Active{ get; set; } =true;
		public string MobileNumber { get; set; } = string.Empty;  
	}
}
