using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using WebApp.Data.Account;
using WebAppAuthentication.Integration.UserProfileAPI;

namespace WebApp.Pages.Account
{
	[Authorize]
	public class ManageProfileModel : PageModel
	{
		//public List<SelectListItem> DepartmentOptions { get; set; } = new List<SelectListItem>();
		//public List<SelectListItem> DesignationOptions { get; set; } = new List<SelectListItem>();
		private readonly UserManager<IdentityUserExtension> userManager;
		private readonly IUserProfileService userProfileService;

		[BindProperty]
		public ManageProfileViewModel UserProfileView { get; set; }

		[BindProperty]
		public string? SuccessMessage { get; set; }
		[BindProperty]
		public bool IsProfileExist { get; set; } = true;

		[BindProperty]
		public string Email { get; set; } = string.Empty;

		[BindProperty]
		public IFormFile? UploadedPicture { get; set; }
		public byte[]? CurrentPictureBytes { get; set; }

		private const long MaxFileSizeBytes = 2 * 1024 * 1024;
		private readonly string[] PermittedExtensions = { ".png", ".jpg", ".jpeg" };

		public ManageProfileModel(UserManager<IdentityUserExtension> _userManager, IUserProfileService _userProfileService)
		{
			userManager = _userManager;
			userProfileService = _userProfileService;
			UserProfileView = new ManageProfileViewModel(); 
		}

		public async Task<IActionResult> OnGetAsync()
		{
			if (!ModelState.IsValid)
				return Page();

			SuccessMessage = string.Empty;
			//PopulateDropdownOptions();

			if (User.Identity?.IsAuthenticated == true)
			{
				Email = User.Identity.Name ?? string.Empty;
				var user = await userManager.GetUserAsync(User);

				if (user is not null)
				{
					IsProfileExist = user.IsProfileExist;
					CurrentPictureBytes = user.ProfilePicture;

					// invoke userProfileService to get custom details
					var userDetailsResponse = await userProfileService.GetUserProfilePayload(Guid.Parse(user.Id)); 
					if (userDetailsResponse is not null)
					{
						string statusCode = ((JsonResult)userDetailsResponse).StatusCode.ToString()??"500";
						if (statusCode == "200")
						{
							try
							{
								var result = JsonSerializer.Deserialize<UserProfile>(((JsonResult)userDetailsResponse).Value.ToString());
								IsProfileExist = true;

								UserProfileView.FirstName = result.FirstName??string.Empty;
								UserProfileView.LastName = result.LastName ?? string.Empty;
								UserProfileView.MobileNumber = result.MobileNumber ?? string.Empty;
								UserProfileView.DateOfBirth = result.DateOfBirth;
							}
							catch (Exception ex)
							{
								ModelState.AddModelError("ManageProfile","Something went wrong, please try again.");
								// log exception ex
							}
						}
						else if(statusCode == "404") 
							IsProfileExist = false; 
						else
							ModelState.AddModelError("ManageProfile", "Something went wrong, please try again.");
					}
				}
			}
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (UploadedPicture is not null)
			{
				if (UploadedPicture.Length > MaxFileSizeBytes)
					ModelState.AddModelError("UploadedPicture", "The profile picture file size must not exceed 2 MB.");

				var fileExtension = Path.GetExtension(UploadedPicture.FileName).ToLowerInvariant();
				if (string.IsNullOrEmpty(fileExtension) || !PermittedExtensions.Contains(fileExtension))
					ModelState.AddModelError("UploadedPicture", "Only PNG, JPG, or JPEG file formats are permitted.");
			}

			var user = await userManager.FindByEmailAsync(Email);
			if (!ModelState.IsValid)
			{
				//PopulateDropdownOptions();
				CurrentPictureBytes = user?.ProfilePicture;
				return Page();
			}

			try
			{
				if (user is null)
					return NotFound();

				IsProfileExist = user.IsProfileExist;

				UserProfile userProfile = new()
				{
					Id = Guid.Parse(user.Id),
					FirstName = UserProfileView.FirstName,
					LastName = UserProfileView.LastName,
					MobileNumber = UserProfileView.MobileNumber,
					DateOfBirth = UserProfileView.DateOfBirth ?? DateTime.MinValue
				};

				string state = "created";
				IsProfileExist = await userProfileService.UpsertUserProfileAsync(userProfile, IsProfileExist);
				user.IsProfileExist = true;
				await userManager.UpdateAsync(user);

				if (UploadedPicture is not null)
				{
					using (var memoryStream = new MemoryStream())
					{
						await UploadedPicture.CopyToAsync(memoryStream);
						user.ProfilePicture = memoryStream.ToArray();

						await userManager.UpdateAsync(user);
						CurrentPictureBytes = user.ProfilePicture;
					}
				}

				SuccessMessage = $"User profile {state} successfully.";
			}
			catch (Exception)
			{
				ModelState.AddModelError("User Profile", "An error occurred while creating/updating the user profile.");
			}

			return Page();
		}

		public async Task<IActionResult> OnPostRemovePictureAsync()
		{
			//var user = await userManager.FindByEmailAsync(Email);
			//if (user is not null)
			//{
			//	user.ProfilePicture = null;
			//	await userManager.UpdateAsync(user);
			//}

			//SuccessMessage = "Profile picture removed successfully.";
			return RedirectToPage();
		}

		private async Task<(UserProfile? user, Claim? departmentClaim, Claim? designationClaim)> GetUserInfoAsync(string email)
		{
			//var user = await userManager.FindByEmailAsync(email);
			//if (user is not null)
			//{
			//	var claims = await userManager.GetClaimsAsync(user);
			//	var departmentClaim = claims.FirstOrDefault(c => c.Type == "Department");
			//	var designationClaim = claims.FirstOrDefault(c => c.Type == "Designation");
			//	return (user, departmentClaim, designationClaim);
			//}
			return (null, null, null);
		}

		//private void PopulateDropdownOptions()
		//{
		//	DepartmentOptions = new List<SelectListItem>
		//	{
		//		new SelectListItem { Value = "", Text = "-- Select Department --" },
		//		new SelectListItem { Value = "HR", Text = "Human Resources" },
		//		new SelectListItem { Value = "IT", Text = "Information Technology" },
		//		new SelectListItem { Value = "Finance", Text = "Finance" },
		//		new SelectListItem { Value = "Marketing", Text = "Marketing" }
		//	};

		//	DesignationOptions = new List<SelectListItem>
		//	{
		//		new SelectListItem { Value = "", Text = "-- Select Designation --" },
		//		new SelectListItem { Value = "Manager", Text = "Manager" },
		//		new SelectListItem { Value = "Developer", Text = "Developer" },
		//		new SelectListItem { Value = "Analyst", Text = "Analyst" },
		//		new SelectListItem { Value = "Executive", Text = "Executive" }
		//	};
		//}
	}


	public class ManageProfileViewModel
	{
		[Required]
		[Display(Name = "First Name")]
		public string FirstName { get; set; } = string.Empty;

		[Required]
		[Display(Name = "Last Name")]
		public string LastName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Mobile number is required.")]
		[Display(Name = "Mobile Number")]
		// Validates 9 digits starting with 6, 7 or 8 (allows space/hyphen splits optionally)
		[RegularExpression(@"^[678]\d{8}$|^[678]\d{2}[\s-]?\d{3}[\s-]?\d{4}$", ErrorMessage = "Enter a valid 9-digit South African mobile number (e.g. 712345678).")]
		public string MobileNumber { get; set; } = string.Empty;
		
		[Required]
		[DataType(DataType.Date)]
		//[MinimumAge(18, ErrorMessage = "You must be 18 years or older.")]
		[Display(Name = "Date of Birth")]		public DateTime? DateOfBirth { get; set; }

		//[Required(ErrorMessage = "Please select a department.")]
		//public string Department { get; set; } = string.Empty;

		//[Required(ErrorMessage = "Please select a designation.")]
		//public string Designation { get; set; } = string.Empty;
	} 
}