using Dapper;
using DapperQueryService.Core;
using System.Data;
using System.Security.Cryptography.Xml;

namespace DapperQueryService.Infra
{
	public class UserTransactions : IUserTransactions
	{
		private readonly IDbConnection _dbConnection;
		public UserTransactions(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
		}

		public async Task<bool> AddTransactionAsync(TransactionEntitySQL transactionEntity)
		{
			const string sql = @"
			INSERT INTO UserTransactions (Id, ReferenceId, Description, TransactionType, TransactionDate, Amount)
			VALUES (@Id, @ReferenceId, @Description, @TransactionType, @TransactionDate, @Amount)";

			await _dbConnection.ExecuteAsync(sql, transactionEntity);
			return true;
		}

		public async Task<(IEnumerable<TransactionEntitySQL> Transactions, int TotalCount)> GetTransactionsAsync(Guid referenceId, int? pageNumber = 1, int? pageSize = 10)
		{
			// Combine both count and paginated query into a single batch execution
			const string sql = @"
			SELECT COUNT(*) FROM UserTransactions WHERE ReferenceId = @ReferenceId;

			SELECT Id, ReferenceId, Description, TransactionType, TransactionDate, Amount
			FROM UserTransactions 
			WHERE ReferenceId = @ReferenceId
			ORDER BY Id
			OFFSET @Offset ROWS
			FETCH NEXT @PageSize ROWS ONLY;";

			// Fallback safe calculations
			int actualPageNumber = pageNumber ?? 1;
			int actualPageSize = pageSize ?? 10;
			var offset = (actualPageNumber - 1) * actualPageSize;

			using var multi = await _dbConnection.QueryMultipleAsync(sql, new
			{
				ReferenceId = referenceId,
				Offset = offset,
				PageSize = actualPageSize
			});

			int totalCount = await multi.ReadFirstAsync<int>();
			var transactions = await multi.ReadAsync<TransactionEntitySQL>();

			return (transactions, totalCount);
		}

		public async Task<IEnumerable<TransactionEntitySQL>> GetTransactionsAsyncOld(Guid referenceId, int? pageNumber = 1, int? pageSize = 10)
		{
			const string sql = @"
			SELECT Id, ReferenceId, Description, TransactionType, TransactionDate, Amount
			FROM UserTransactions 
			WHERE ReferenceId=@ReferenceId
			ORDER BY Id
			OFFSET @offset ROWS
			FETCH NEXT @pageSize  ROWS ONLY";

			var offset = (pageNumber - 1) * pageSize;
			return await _dbConnection.QueryAsync<TransactionEntitySQL>(sql, new { ReferenceId= referenceId, Offset = offset, PageSize = pageSize });
		}

		public async Task<TransactionEntitySQL> GetTransactionByIdAsync(Guid id)
		{
			const string sql = @"
				SELECT Id, ReferenceId, Description, TransactionType, TransactionDate, Amount
			FROM UserTransactions
			WHERE  Id = @Id";

			var userTransactionEntity = await _dbConnection.QueryFirstOrDefaultAsync<TransactionEntitySQL>(sql, new { Id = id });
			if (userTransactionEntity is null)
				return new TransactionEntitySQL();

			return userTransactionEntity;
		}
	}
}
