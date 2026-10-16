using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace WebAppAuthentication.Pages.Transactions
{
	public class ViewTransactionsModel : PageModel
	{
		[BindProperty(SupportsGet = true)]
		public bool IsStackLocked { get; set; } = false;

		[BindProperty(SupportsGet = true)]
		public string SelectedDb { get; set; } = "SqlServer"; // Default option fallback
																													// Bind properties to GET requests seamlessly
		[BindProperty(SupportsGet = true)]
		public string Email { get; set; } = string.Empty;

		[BindProperty(SupportsGet = true)]
		public string SelectedOrm { get; set; } = "EFCore"; // Fallback default

		[BindProperty(SupportsGet = true)]
		public DateTime? StartDate { get; set; }

		[BindProperty(SupportsGet = true)]
		public DateTime? EndDate { get; set; }

		[BindProperty(SupportsGet = true)]
		public string? TransactionType { get; set; }

		[BindProperty(SupportsGet = true)]
		public int CurrentPage { get; set; } = 1;

		public int PageSize { get; set; } = 5;
		public int TotalPages { get; set; }
		public long ExecutionTimeMs { get; set; }

		public List<TransactionItem> Transactions { get; set; } = new();

		public bool HasPreviousPage => CurrentPage > 1;
		public bool HasNextPage => CurrentPage < TotalPages;

		public async Task<IActionResult> OnGetAsync()
		{
			var action = Request.Query["Action"].ToString();

			if (action == "LockStack")
			{
				IsStackLocked = true;
			}
			else if (action == "UnlockStack")
			{
				IsStackLocked = false;
			}

			var stopwatch = Stopwatch.StartNew();

			// 1. Standard fallback constraint updates
			if (CurrentPage < 1) CurrentPage = 1;

			// 2. Query Mock Dataset (Replace with database contexts filtered conditionally)
			var rawData = GetMockTransactions();

			// Apply Filters dynamically
			if (StartDate.HasValue) rawData = rawData.Where(t => t.Timestamp >= StartDate.Value).ToList();
			if (EndDate.HasValue) rawData = rawData.Where(t => t.Timestamp <= EndDate.Value.AddDays(1)).ToList();
			if (!string.IsNullOrEmpty(TransactionType)) rawData = rawData.Where(t => t.Type == TransactionType).ToList();

			// 3. Compute Pagination Metadata Numbers
			int totalItems = rawData.Count;
			TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

			// Fetch active sliding page data segment
			Transactions = rawData
					.Skip((CurrentPage - 1) * PageSize)
					.Take(PageSize)
					.ToList();

			stopwatch.Stop();
			ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

			return Page();
		}

		// Simplifies route payload state mappings on pagination links 
		public Dictionary<string, string> GetRouteData(int targetPage)
		{
			var dict = new Dictionary<string, string>
						{
								{ "CurrentPage", targetPage.ToString() },
								{ "SelectedOrm", SelectedOrm },
								{ "Email", Email }
						};

			if (StartDate.HasValue) dict.Add("StartDate", StartDate.Value.ToString("yyyy-MM-dd"));
			if (EndDate.HasValue) dict.Add("EndDate", EndDate.Value.ToString("yyyy-MM-dd"));
			if (!string.IsNullOrEmpty(TransactionType)) dict.Add("TransactionType", TransactionType);
			dict.Add("SelectedDb", SelectedDb);
			return dict;
		}

		private List<TransactionItem> GetMockTransactions()
		{
			return new List<TransactionItem>
						{
								new() { Id = 1001, Timestamp = DateTime.Now.AddDays(-2), Reference = "Vendor Invoice payout", Type = "Withdrawal", Amount = 450.00m },
								new() { Id = 1002, Timestamp = DateTime.Now.AddDays(-1), Reference = "Customer Deposit client_a", Type = "Deposit", Amount = 1250.50m },
								new() { Id = 1003, Timestamp = DateTime.Now, Reference = "Online Payment Transfer", Type = "Transfer", Amount = 85.00m },
								new() { Id = 1004, Timestamp = DateTime.Now.AddDays(-4), Reference = "ATM Cash Withdrawal", Type = "Withdrawal", Amount = 200.00m },
								new() { Id = 1005, Timestamp = DateTime.Now.AddDays(-6), Reference = "Stripe Processing Payout", Type = "Deposit", Amount = 3400.00m },
								new() { Id = 1006, Timestamp = DateTime.Now.AddDays(-7), Reference = "Cloud Infra Subscription", Type = "Withdrawal", Amount = 120.00m }
						};
		}
	}

	public class TransactionItem
	{
		public int Id { get; set; }
		public DateTime Timestamp { get; set; }
		public string Reference { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
		public decimal Amount { get; set; }
	}
}