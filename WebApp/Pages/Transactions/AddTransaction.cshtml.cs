using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Account;
using WebAppAuthentication.Data.Entities;
using WebAppAuthentication.Integration.DapperService;

namespace WebAppAuthentication.Pages.Transactions
{
	[Authorize]
	public class AddTransactionModel : PageModel
	{
		private readonly UserManager<IdentityUserExtension> userManager;
		private readonly IDapperSqlService dapperSqlService;
		public AddTransactionModel(UserManager<IdentityUserExtension> _userManager, IDapperSqlService _dapperSqlService)
		{
			userManager = _userManager;
			dapperSqlService = _dapperSqlService;
		}

		[BindProperty]
		public TransactionInput Input { get; set; } = new();

		public Guid UserReferenceId { get; set; }

		public void OnGet()
		{
			Input.TransactionDate = DateTime.Today;
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (User.Identity?.IsAuthenticated == true)
			{
				var user = await userManager.GetUserAsync(User);
				if (user is null)
				{
					ModelState.AddModelError(string.Empty, "User details are not found. Please logout and login again.");
					return Page();
				}
				UserReferenceId = Guid.Parse(user.Id);
			}

			if (!ModelState.IsValid)
				return Page();

			try
			{
				UserTransactionEntity userTransactionEntity = new UserTransactionEntity
				{
					Id = Guid.NewGuid(),
					ReferenceId = UserReferenceId,
					Description = Input.Description,
					TransactionType = Input.TransactionType,
					TransactionDate = Input.TransactionDate,
					Amount = Input.Amount
				};

				IActionResult result = await dapperSqlService.CreateUserTransaction(userTransactionEntity);

				if (result is ObjectResult objectResult)
				{
					int status = objectResult.StatusCode ?? StatusCodes.Status500InternalServerError;

					if (status == StatusCodes.Status201Created)
					{
						// Set success indicators for the front-end popup
						TempData["PopupType"] = "success";
						TempData["PopupMessage"] = "Transaction has been created successfully!";

						// Clear the form for the next entry by redirecting back to the clean GET path
						return RedirectToPage();
					}

					// Map and handle API errors gracefully via standard Alert components
					string errorMessage = objectResult.Value?.ToString() ?? "An unexpected transaction error occurred.";
					if (status == StatusCodes.Status409Conflict)
						errorMessage = "Duplicate detected: This transaction reference already exists.";

					TempData["PopupType"] = "error";
					TempData["PopupMessage"] = errorMessage;
					return Page();
				}

				TempData["PopupType"] = "error";
				TempData["PopupMessage"] = "Received an unmanageable response structure from the backend engine.";
				return Page();
			}
			catch (Exception ex)
			{
				TempData["PopupType"] = "error";
				TempData["PopupMessage"] = $"An error occurred while saving: {ex.Message}";
				return Page();
			}
		}
	}

	public class TransactionInput
	{
		[Required(ErrorMessage = "Transaction Date is required.")]
		[DataType(DataType.Date)]
		public DateTime TransactionDate { get; set; }

		[Required(ErrorMessage = "Please select a Transaction Type.")]
		public string TransactionType { get; set; } = string.Empty;

		[Required(ErrorMessage = "Amount is required.")]
		[Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
		[DataType(DataType.Currency)]
		public decimal Amount { get; set; }

		[Required(ErrorMessage = "Reference / Description is required.")]
		[StringLength(250, ErrorMessage = "Reference cannot exceed 250 characters.")]
		public string Description { get; set; } = string.Empty;
	}
}