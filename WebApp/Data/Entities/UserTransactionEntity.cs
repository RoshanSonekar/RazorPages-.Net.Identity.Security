using System.Text.Json.Serialization;

namespace WebAppAuthentication.Data.Entities
{
	public class UserTransactionEntity
	{
		[JsonPropertyName("id")]
		public Guid Id { get; set; }

		[JsonPropertyName("referenceId")]
		public Guid ReferenceId { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; } = string.Empty;

		[JsonPropertyName("transactionType")]
		public string TransactionType { get; set; } = string.Empty;

		[JsonPropertyName("transactionDate")]
		public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

		[JsonPropertyName("amount")]
		public decimal Amount { get; set; } = 0.0M;

		//public Guid Id { get; set; }

		//public Guid ReferenceId { get; set; }

		//public string Description { get; set; } = string.Empty;

		//public string TransactionType { get; set; } = string.Empty;

		//public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

		//public double Amount { get; set; } = 0.0;
	}
}
