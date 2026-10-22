using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using WebApp.Data.Account;
using WebAppAuthentication.Integration.DapperService;

namespace WebAppAuthentication.Pages.Transactions
{ 
	[Authorize]
	public class ViewTransactionsModel : PageModel
	{
		private readonly UserManager<IdentityUserExtension> userManager;
		private readonly IDapperSqlService dapperSqlService;
		public ViewTransactionsModel(UserManager<IdentityUserExtension> _userManager, IDapperSqlService _dapperSqlService)
		{
			userManager = _userManager;
			dapperSqlService = _dapperSqlService;
		}

		[BindProperty]
		public bool IsSearchTrigger { get; set; } = false;

		[BindProperty]
		public Guid UserReferenceId { get; set; }

		[BindProperty(SupportsGet = true)]
		public DateTime? StartDate { get; set; }

		[BindProperty(SupportsGet = true)]
		public DateTime? EndDate { get; set; }

		[BindProperty(SupportsGet = true)]
		public string? TransactionType { get; set; }

		[BindProperty(SupportsGet = true)]
		public int CurrentPage { get; set; } = 1;

		public int PageSize { get; set; } = 5; // Rows matching UI layout container snippet
		public int TotalPages { get; set; }
		public long ExecutionTimeMs { get; set; }

		public List<TransactionItem> Transactions { get; set; } = new();

		public bool HasPreviousPage => CurrentPage > 1;
		public bool HasNextPage => CurrentPage < TotalPages;

		public async Task<IActionResult> OnGetAsync()
		{
			UserReferenceId = Guid.Empty;

			if (User.Identity?.IsAuthenticated == true)
			{
				string Email = User.Identity.Name ?? string.Empty;
				var user = await userManager.GetUserAsync(User);

				if (user is null)
				{
					ModelState.AddModelError("ViewTransactions", "User details are not found, please logout and theb login again to view this page.");
					return Page();
				}
				UserReferenceId = Guid.Parse(user.Id);
			}
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (User.Identity?.IsAuthenticated == true)
			{
				var user = await userManager.GetUserAsync(User);
				if (user is null)
				{
					ModelState.AddModelError("ViewTransactions", "User details are not found, please logout and theb login again to view this page.");
					return Page();
				}
				UserReferenceId = Guid.Parse(user.Id);
			}

			if (string.IsNullOrWhiteSpace(UserReferenceId.ToString()))
				ModelState.AddModelError("ViewTransactions", "User details are not found, please logout and theb login again to view this page.");

			var stopwatch = Stopwatch.StartNew();
			// Reset back to page 1 ONLY if the primary search filter button was pressed.
			if (IsSearchTrigger)
				CurrentPage = 1;

			await FetchPaginatedTransactions();
			stopwatch.Stop();
			ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

			return Page();
		}
		 

		private async Task FetchPaginatedTransactions()
		{
			var stopwatch = Stopwatch.StartNew();

			if (CurrentPage < 1) CurrentPage = 1;

			// Pass down active dynamic pagination tracking variables directly to SQL endpoint layers
			var apiResult = await dapperSqlService.GetUserTransactionsWithMetadata(UserReferenceId, CurrentPage, PageSize);

			var transactionItems = new List<TransactionItem>();
			foreach (var transaction in apiResult.Transactions)
			{
				transactionItems.Add(new TransactionItem
				{
					Id = transaction.Id.ToString(),
					Timestamp = transaction.TransactionDate,
					Reference = transaction.Description,
					Type = transaction.TransactionType,
					Amount = (decimal)transaction.Amount
				});
			}

			// Apply in-memory client processing filters if needed
			if (StartDate.HasValue) transactionItems = transactionItems.Where(t => t.Timestamp >= StartDate.Value).ToList();
			if (EndDate.HasValue) transactionItems = transactionItems.Where(t => t.Timestamp <= EndDate.Value.AddDays(1)).ToList();
			if (!string.IsNullOrEmpty(TransactionType)) transactionItems = transactionItems.Where(t => t.Type == TransactionType).ToList();

			Transactions = transactionItems;

			// If backend doesn't support aggregate count properties, use virtual length matching layout behaviors safely
			int totalItems = apiResult.TotalCount > 0 ? apiResult.TotalCount : 100; // Simulated ceiling or assign totalItems = apiResult.Transactions.Count if unpaginated stream
			TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

			stopwatch.Stop();
			ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
		}

		public Dictionary<string, string> GetRouteData(int targetPage)
		{
			var dict = new Dictionary<string, string>
			{
				{ "CurrentPage", targetPage.ToString() },
			};

			if (StartDate.HasValue) dict.Add("StartDate", StartDate.Value.ToString("yyyy-MM-dd"));
			if (EndDate.HasValue) dict.Add("EndDate", EndDate.Value.ToString("yyyy-MM-dd"));
			if (!string.IsNullOrEmpty(TransactionType)) dict.Add("TransactionType", TransactionType);

			return dict;
		}
	}

	public class TransactionItem
	{
		public string Id { get; set; }
		public DateTime Timestamp { get; set; }
		public string Reference { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
		public decimal Amount { get; set; }
	}

}
