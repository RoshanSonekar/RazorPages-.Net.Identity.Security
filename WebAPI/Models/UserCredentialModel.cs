using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace AuthService.Models;
public class UserCredentialModel
{
	[Required]
	[Display(Name = "Password")]
	[DataType(DataType.Password)]
	public string Password { get; set; } = string.Empty;

	[Required]
	[Display(Name = "Email")]
	[EmailAddress(ErrorMessage = "Invalid email address.")]
	public string Email { get; set; } = string.Empty;
}

public class ApplicationUser : IdentityUser
{
	public string? RefreshToken { get; set; }
	public DateTime RefreshTokenExpiryTime { get; set; }
}

public record TokenRequestModel(string AccessToken, string RefreshToken);
public record TokenResponse(string AccessToken, string RefreshToken);