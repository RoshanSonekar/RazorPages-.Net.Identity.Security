using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserProfileService.Data.DTO
{
	[Table("UserProfile")]
	public class DtoUserProfile
	{
		[Required]
		public Guid Id { get; set; }
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public DateTime DateOfBirth { get; set; }
		public bool Active { get; set; } = true;
		[Phone]
		public string MobileNumber { get; set; } = string.Empty;
		public string Payload { get; set; } = string.Empty;
	}
}
