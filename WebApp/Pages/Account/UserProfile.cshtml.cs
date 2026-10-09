using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
	[Authorize]
	public class UserProfileModel : PageModel
	{
		private readonly UserManager<User> userManager;

		[BindProperty]
		public UserProfileViewModel UserProfileView { get; set; }

		[BindProperty]
		public string? SuccessMessage { get; set; }

		[BindProperty]
		public string Email { get; set; } = string.Empty;

		// Properties for handling profile picture uploading and rendering
		[BindProperty]
		public IFormFile? UploadedPicture { get; set; }
		public byte[]? CurrentPictureBytes { get; set; }

		// Strict validation constraints
		private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB
		private readonly string[] PermittedExtensions = { ".png", ".jpg", ".jpeg" };

		public UserProfileModel(UserManager<User> _userManager)
		{
			userManager = _userManager;
			UserProfileView = new UserProfileViewModel();
		}

		public async Task<IActionResult> OnGetAsync()
		{
			SuccessMessage = string.Empty;
			if (User.Identity?.IsAuthenticated == true)
			{
				Email = User.Identity.Name ?? string.Empty;
				var (user, departmentClaim, designationClaim) = await GetUserInfoAsync(Email);

				if (user is not null)
				{
					UserProfileView.Department = departmentClaim?.Value ?? string.Empty;
					UserProfileView.Designation = designationClaim?.Value ?? string.Empty;
					CurrentPictureBytes = user.ProfilePicture; // Fetch binary from SQL
				}
			}
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			// 1. Run custom file validation checks before validating the ModelState
			if (UploadedPicture is not null)
			{
				if (UploadedPicture.Length > MaxFileSizeBytes)
				{
					ModelState.AddModelError("UploadedPicture", "The profile picture file size must not exceed 2 MB.");
				}

				var fileExtension = Path.GetExtension(UploadedPicture.FileName).ToLowerInvariant();
				if (string.IsNullOrEmpty(fileExtension) || !PermittedExtensions.Contains(fileExtension))
				{
					ModelState.AddModelError("UploadedPicture", "Only PNG, JPG, or JPEG file formats are permitted.");
				}
			}

			if (!ModelState.IsValid)
			{
				// Reload picture from the database so it stays visible on validation failure
				var rawUser = await userManager.FindByEmailAsync(Email);
				CurrentPictureBytes = rawUser?.ProfilePicture;
				return Page();
			}

			try
			{
				var (user, departmentClaim, designationClaim) = await GetUserInfoAsync(Email);
				if (user is null)
					return NotFound();

				// Update Department Claim
				if (departmentClaim is not null)
					await userManager.ReplaceClaimAsync(user, departmentClaim, new Claim(departmentClaim.Type, UserProfileView.Department ?? string.Empty));
				else
					await userManager.AddClaimAsync(user, new Claim("Department", UserProfileView.Department ?? string.Empty));

				// Update Designation Claim
				if (designationClaim is not null)
					await userManager.ReplaceClaimAsync(user, designationClaim, new Claim(designationClaim.Type, UserProfileView.Designation ?? string.Empty));
				else
					await userManager.AddClaimAsync(user, new Claim("Designation", UserProfileView.Designation ?? string.Empty));

				// 2. Process and save the file to SQL Server if provided
				if (UploadedPicture is not null)
				{
					using (var memoryStream = new MemoryStream())
					{
						await UploadedPicture.CopyToAsync(memoryStream);
						user.ProfilePicture = memoryStream.ToArray();
					}
					await userManager.UpdateAsync(user);
					CurrentPictureBytes = user.ProfilePicture;
				}
				else
				{
					// Retain current image binary data if no new file is uploaded
					var freshUser = await userManager.FindByEmailAsync(Email);
					CurrentPictureBytes = freshUser?.ProfilePicture;
				}

				SuccessMessage = "User profile updated successfully.";
			}
			catch (Exception)
			{
				ModelState.AddModelError("User Profile", "An error occurred while updating the user profile.");
			}

			return Page();
		}

		// Explicit handler to clear image bytes from the user record
		public async Task<IActionResult> OnPostRemovePictureAsync()
		{
			var user = await userManager.FindByEmailAsync(Email);
			if (user is not null)
			{
				user.ProfilePicture = null;
				await userManager.UpdateAsync(user);
			}

			SuccessMessage = "Profile picture removed successfully.";
			return RedirectToPage();
		}

		private async Task<(User? user, Claim? departmentClaim, Claim? designationClaim)> GetUserInfoAsync(string email)
		{
			var user = await userManager.FindByEmailAsync(email);
			if (user is not null)
			{
				var claims = await userManager.GetClaimsAsync(user);
				var departmentClaim = claims.FirstOrDefault(c => c.Type == "Department");
				var designationClaim = claims.FirstOrDefault(c => c.Type == "Designation");
				return (user, departmentClaim, designationClaim);
			}
			return (null, null, null);
		}
	}

	public class UserProfileViewModel
	{
		[Required]
		public string? Department { get; set; }

		[Required]
		public string? Designation { get; set; }
	}
}