using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Text.Json.Serialization;
using WebAppAuthentication.Data.Entities;

namespace WebAppAuthentication.Integration.DapperService
{
	public class PagedApiResponse
	{
		[JsonPropertyName("transactionEntities")] // <-- Change this to match the actual JSON key (e.g., "data" or "items")
		public List<UserTransactionEntity> Transactions { get; set; } = [];

		[JsonPropertyName("totalCount")]
		public int TotalCount { get; set; }
	}

	public class DapperSqlService : IDapperSqlService
	{
		private readonly IHttpClientFactory httpClientFactory;
		private readonly ILogger<DapperSqlService> logger;

		public DapperSqlService(IHttpClientFactory _httpClientFactory, ILogger<DapperSqlService> _logger)
		{
			httpClientFactory = _httpClientFactory;
			logger = _logger;
		}

		public async Task<PagedApiResponse> GetUserTransactionsWithMetadata(Guid TransactionId, int? pageNumber = 1, int? pageSize = 5)
		{
			string baseUri = $"UserTransactionsSql/{TransactionId}";

			var queryParams = new Dictionary<string, string?>
			{
				{ "PageNumber", pageNumber.ToString() },
				{ "PageSize", pageSize.ToString() }
			};

			string requestUri = QueryHelpers.AddQueryString(baseUri, queryParams);

			var client = httpClientFactory.CreateClient("UserTransactionDapperService");
			using HttpResponseMessage response = await client.GetAsync(requestUri);

			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadFromJsonAsync<PagedApiResponse>();
				return result ?? new PagedApiResponse();
			}

			return new PagedApiResponse();
		}

		public async Task<List<UserTransactionEntity>> GetUserTransactions(Guid TransactionId, int? pageNumber = 1, int? pageSize = 5)
		{
			string baseUri = $"UserTransactionsSql/{TransactionId}";

			var queryParams = new Dictionary<string, string?>
				{
						{ "PageNumber", pageNumber?.ToString() ?? "1" },
						{ "PageSize", pageSize?.ToString() ?? "5" }
				};

			string requestUri = QueryHelpers.AddQueryString(baseUri, queryParams);

			var client = httpClientFactory.CreateClient("UserTransactionDapperService");
			using HttpResponseMessage response = await client.GetAsync(requestUri);

			if (response.IsSuccessStatusCode)
			{
				var result = await response.Content.ReadFromJsonAsync<PagedApiResponse>();
				return result?.Transactions ?? [];
			}

			return [];
		}

		public async Task<IActionResult> GetUserTransaction(Guid TransactionId)
		{
			var client = httpClientFactory.CreateClient("UserTransactionDapperService");
			string requestUri = $"UserTransactionSql/{TransactionId}";

			using HttpResponseMessage response = await client.GetAsync(requestUri);

			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				var errorBody = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
				string message = errorBody?.GetValueOrDefault("message") ?? "User not found.";

				return new JsonResult(new { message }) { StatusCode = StatusCodes.Status404NotFound };
			}
			else if (!response.IsSuccessStatusCode)
			{
				var errorBody = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
				string message = errorBody?.GetValueOrDefault("message") ?? "The downstream user transaction service returned an error.";

				return new JsonResult(new { message }) { StatusCode = StatusCodes.Status500InternalServerError };
			}

			string content = await response.Content.ReadAsStringAsync();
			return new JsonResult(content) { StatusCode = StatusCodes.Status200OK };
		}

	}
}
