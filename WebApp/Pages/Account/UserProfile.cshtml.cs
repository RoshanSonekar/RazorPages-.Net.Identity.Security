using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using WebApp.Data.Account;

namespace WebApp.Pages.Account
{
	[Authorize]
	public class UserProfileModel : PageModel
	{
		// Lists to hold dropdown items for the UI
		public List<SelectListItem> DepartmentOptions { get; set; } = new List<SelectListItem>();
		public List<SelectListItem> DesignationOptions { get; set; } = new List<SelectListItem>();
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
			PopulateDropdownOptions();
			if (User.Identity?.IsAuthenticated == true)
			{
				Email = User.Identity.Name ?? string.Empty;
				var (user, departmentClaim, designationClaim) = await GetUserInfoAsync(Email);

				if (user is not null)
				{
					// Map Core properties from User Entity to ViewModel 
					UserProfileView.FirstName = user.FirstName;
					UserProfileView.LastName = user.LastName;
					UserProfileView.DateOfBirth = user.DateOfBirth;

					// Map Claims
					UserProfileView.Department = departmentClaim?.Value ?? string.Empty;
					UserProfileView.Designation = designationClaim?.Value ?? string.Empty;

					CurrentPictureBytes = user.ProfilePicture;
				}
			}
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			// 1. Run custom file validation checks
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
				// Repopulate UI dependencies on execution failures
				PopulateDropdownOptions();
				var rawUser = await userManager.FindByEmailAsync(Email);
				CurrentPictureBytes = rawUser?.ProfilePicture;
				return Page();
			}

			try
			{
				var (user, departmentClaim, designationClaim) = await GetUserInfoAsync(Email);
				if (user is null)
					return NotFound();

				// Update baseline context variables directly on identity record
				user.FirstName = UserProfileView.FirstName;
				user.LastName = UserProfileView.LastName;
				user.DateOfBirth = UserProfileView.DateOfBirth ?? DateTime.MinValue;

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

				// 2. Process and save the file if provided
				if (UploadedPicture is not null)
				{
					using (var memoryStream = new MemoryStream())
					{
						await UploadedPicture.CopyToAsync(memoryStream);
						user.ProfilePicture = memoryStream.ToArray();
					}
				}

				// Commit core properties changes to database
				await userManager.UpdateAsync(user);
				CurrentPictureBytes = user.ProfilePicture;

				SuccessMessage = "User profile updated successfully.";
			}
			catch (Exception)
			{
				ModelState.AddModelError("User Profile", "An error occurred while updating the user profile.");
			}

			PopulateDropdownOptions();
			return Page();
		}

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

		private void PopulateDropdownOptions()
		{
			DepartmentOptions = new List<SelectListItem>
						{
								new SelectListItem { Value = "", Text = "-- Select Department --" },
								new SelectListItem { Value = "HR", Text = "Human Resources" },
								new SelectListItem { Value = "IT", Text = "Information Technology" },
								new SelectListItem { Value = "Finance", Text = "Finance" },
								new SelectListItem { Value = "Marketing", Text = "Marketing" }
						};

			DesignationOptions = new List<SelectListItem>
						{
								new SelectListItem { Value = "", Text = "-- Select Designation --" },
								new SelectListItem { Value = "Manager", Text = "Manager" },
								new SelectListItem { Value = "Developer", Text = "Developer" },
								new SelectListItem { Value = "Analyst", Text = "Analyst" },
								new SelectListItem { Value = "Executive", Text = "Executive" }
						};
		}
	}

	public class UserProfileViewModel
	{
		[Required]
		[Display(Name = "First Name")]
		public string FirstName { get; set; } = string.Empty;

		[Required]
		[Display(Name = "Last Name")]
		public string LastName { get; set; } = string.Empty;

		[Required]
		[DataType(DataType.Date)]
		[MinimumAge(18, ErrorMessage = "You must be 18 years or older.")]
		[Display(Name = "Date of Birth")]
		public DateTime? DateOfBirth { get; set; }

		[Required(ErrorMessage = "Please select a department.")]
		public string Department { get; set; } = string.Empty;

		[Required(ErrorMessage = "Please select a designation.")]
		public string Designation { get; set; } = string.Empty;
	} 
}