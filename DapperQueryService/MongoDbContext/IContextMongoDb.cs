using DapperQueryService.Core;
using MongoDB.Driver;

namespace DapperQueryService.MongoDbContext
{
	public interface IContextMongoDb
	{
		IMongoCollection<TransactionEntitySQL> UserTransaction{ get; }
	}
}
