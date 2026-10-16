using System.ComponentModel.DataAnnotations.Schema;

namespace DapperQueryService.Core
{
	[Table("UserTransactions")]
	public class TransactionEntityMongoDb
	{
		public Guid Id { get; set; }

		public Guid ReferenceId { get; set; }

		public string Description { get; set; } = string.Empty;

		public string TransactionType { get; set; } = string.Empty;

		public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

		public double Amount { get; set; } = 0.0;
	}
}
