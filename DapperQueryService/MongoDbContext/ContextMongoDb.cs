using DapperQueryService.Core;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace DapperQueryService.MongoDbContext;

public class ContextMongoDb : IContextMongoDb
{
	public IMongoCollection<TransactionEntitySQL> UserTransaction { get; }
	public ContextMongoDb(IConfiguration config)
	{
		var client = new MongoClient(config["DatabaseSettings:MongoDBConnectionString"]);
		var database = client.GetDatabase(config["DatabaseSettings:DatabaseName"]);

		UserTransaction = database.GetCollection<TransactionEntitySQL>(config["DatabaseSettings:CollectionName"]);
	}
}