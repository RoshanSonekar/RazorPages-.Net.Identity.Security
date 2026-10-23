using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DapperQueryService.Core
{
	public class TransactionEntitySQL
	{
		public Guid Id { get; set; }
		public Guid ReferenceId { get; set; }
		public string Description { get; set; } = string.Empty;
		public string TransactionType { get; set; } = string.Empty;
		public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
		public double Amount { get; set; } = 0.0;
	}
}
