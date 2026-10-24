using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Data.Account;
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
			// Sets default date to today for user convenience
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
			{
				return Page();
			}

			try
			{
				// NOTE: Map values cleanly to match your database layer signatures.
				// Assuming an insert pattern matching your fetch scheme:
				// await _dapperSqlService.InsertTransaction(UserReferenceId, Input.TransactionDate, Input.TransactionType, Input.Amount, Input.Description);

				TempData["SuccessMessage"] = "Transaction has been created successfully!";
				return RedirectToPage("./ViewTransactions");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"An error occurred while saving: {ex.Message}");
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