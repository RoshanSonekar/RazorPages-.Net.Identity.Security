using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Account;
using WebApp.Integration.Email;
namespace WebApp.Pages.Account
{
	public class RegisterModel : PageModel
	{
		[BindProperty]
		public RegisterViewModel registerViewModel { get; set; } = new RegisterViewModel();

		// Lists to hold dropdown items for the UI
		public List<SelectListItem> DepartmentOptions { get; set; } = new List<SelectListItem>();
		public List<SelectListItem> DesignationOptions { get; set; } = new List<SelectListItem>();

		private readonly IEmailService emailService;
		private readonly UserManager<User> userManager;

		public RegisterModel(UserManager<User> _userManager, IEmailService _emailService)
		{
			userManager = _userManager;
			emailService = _emailService;
		}

		public void OnGet()
		{
			PopulateDropdownOptions();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!ModelState.IsValid)
			{
				// Repopulate dropdown lists if form validation fails and page reloads
				PopulateDropdownOptions();
				return Page();
			}

			// Create user mapping the new fields (Assumes your Identity 'User' entity has these properties)
			var user = new User
			{
				Email = registerViewModel.Email,
				UserName = registerViewModel.Email,
				FirstName = registerViewModel.FirstName,
				LastName = registerViewModel.LastName,
				DateOfBirth = registerViewModel.DateOfBirth ?? DateTime.MinValue,
				Department = registerViewModel.Department,
				Designation = registerViewModel.Designation
			};

			// Create claims
			List<System.Security.Claims.Claim> claimsList = new List<System.Security.Claims.Claim>
						{
								new System.Security.Claims.Claim("Department", registerViewModel.Department),
								new System.Security.Claims.Claim("Designation", registerViewModel.Designation)
						};

			var result = await userManager.CreateAsync(user, registerViewModel.Password);

			if (result.Succeeded)
			{
				await userManager.AddClaimsAsync(user, claimsList);

				// Generate token and link
				var emailConfirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
				var confirmationLink = Url.PageLink(pageName: "/Account/ConfirmEmail", values: new { userId = user.Id, token = emailConfirmationToken });

				// Send token via email
				await emailService.Send(new EmailConfirmationModelRequest()
				{
					Email = user.Email,
					Subject = "Verify Email",
					MessageBody = $"Please click on this link to confirm your email - {confirmationLink}"
				});

				return RedirectToPage("/Account/SentConfirmEmailLink");
			}
			else
			{
				foreach (var error in result.Errors)
					ModelState.AddModelError("Sign-Up", error.Description);

				PopulateDropdownOptions();
				return Page();
			}
		}

		/// <summary>
		/// Populates the select lists for Departments and Designations.
		/// Replace these mock items with data from your database if necessary.
		/// </summary>
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

	public class MinimumAgeAttribute : ValidationAttribute
	{
		private readonly int _minimumAge;

		public MinimumAgeAttribute(int minimumAge)
		{
			_minimumAge = minimumAge;
		}

		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is DateTime dateOfBirth)
			{
				var today = DateTime.Today;
				var age = today.Year - dateOfBirth.Year;

				if (dateOfBirth.Date > today.AddYears(-age)) age--;

				if (age < _minimumAge)
				{
					return new ValidationResult(ErrorMessage ?? $"You must be at least {_minimumAge} years old.");
				}
			}
			return ValidationResult.Success;
		}
	}

	public class RegisterViewModel
  {
		[Required]
		[Display(Name = "First Name")]
		public string FirstName { get; set; } = string.Empty;

		[Required]
		[Display(Name = "Last Name")]
		public string LastName { get; set; } = string.Empty;

		[Required]
		[DataType(DataType.Date)]
		[MinimumAge(18, ErrorMessage = "You must be 18 years or older to register.")]
		[Display(Name = "Date of Birth")]
		public DateTime? DateOfBirth { get; set; }

		[Required]
		[EmailAddress(ErrorMessage = "Invalid email address.")]
		public string Email { get; set; } = string.Empty;

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; } = string.Empty;

		[Required(ErrorMessage = "Please select a department.")]
		public string Department { get; set; } = string.Empty;

		[Required(ErrorMessage = "Please select a designation.")]
		public string Designation { get; set; } = string.Empty;
	} 
}
