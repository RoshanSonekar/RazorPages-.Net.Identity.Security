using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DapperQueryService.Core
{
	public class TransactionEntitySQL
	{
		[BsonId]
		[BsonRepresentation(BsonType.String)]
		[BsonElement("_id")]
		public Guid Id { get; set; }

		[BsonId]
		[BsonRepresentation(BsonType.String)]
		[BsonElement("ReferenceId")]
		public Guid ReferenceId { get; set; }

		[BsonElement("Description")]
		public string Description { get; set; } = string.Empty;

		[BsonElement("TransactionType")]
		public string TransactionType { get; set; } = string.Empty;

		[BsonElement("TransactionDate")]
		[BsonRepresentation(BsonType.DateTime)]
		public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

		[BsonElement("Amount")]
		[BsonRepresentation(BsonType.Double)]
		public double Amount { get; set; } = 0.0;
	}
}
