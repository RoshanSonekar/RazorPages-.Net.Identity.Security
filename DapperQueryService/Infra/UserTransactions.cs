using Dapper;
using DapperQueryService.Core;
using System.Data;

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

		public async Task<IEnumerable<TransactionEntitySQL>> GetTransactionsAsync(int? pageNumber = 1, int? pageSize = 10)
		{
			const string sql = @"
			SELECT Id, ReferenceId, Description, TransactionType, TransactionDate, Amount
			FROM UserTransactions 
			ORDER BY Id
			OFFSET @offset ROWS
			FETCH NEXT @pageSize  ROWS ONLY";

			var offset = (pageNumber - 1) * pageSize;
			return await _dbConnection.QueryAsync<TransactionEntitySQL>(sql, new { Offset = offset, PageSize = pageSize });
		}

		public async Task<TransactionEntitySQL> GetTransactionByIdAsync(Guid id)
		{
			const string sql = @"
				SELECT Id, ReferenceId, Description, TransactionType, TransactionDate, Amount
			FROM Flights
			WHERE  Id = @Id";

			var userTransactionEntity = await _dbConnection.QueryFirstOrDefaultAsync<TransactionEntitySQL>(sql, new { Id = id });
			if (userTransactionEntity is null)
				return new TransactionEntitySQL();

			return userTransactionEntity;
		}

		public Task<IEnumerable<TransactionEntitySQL>> GetTransactionsAsync(DateTime? FromDate, DateTime? ToDate)
		{
			throw new NotImplementedException();
		}
	}
}
