using Dapper;
using DapperQueryService.Core;
using System.Data;
using System.Security.Cryptography.Xml;
using System.Text;

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
			ORDER BY TransactionDate DESC
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

		public async Task<(IEnumerable<TransactionEntitySQL> Transactions, int TotalCount)> GetTransactionsByFilterAsync(Guid referenceId, DateTime? StartDate = null, DateTime? EndDate = null, int? PageNumber = 1, int? PageSize = 10)
		{
			var sqlBuilder = new StringBuilder();

			// 1. Build the dynamic COUNT query
			sqlBuilder.Append("SELECT COUNT(*) FROM UserTransactions WHERE ReferenceId = @ReferenceId");

			// Shared filter criteria
			string filterCriteria = string.Empty;
			if (StartDate.HasValue && EndDate.HasValue)
			{
				filterCriteria = " AND TransactionDate BETWEEN @StartDate AND @EndDate";
			}
			else if (StartDate.HasValue)
			{
				filterCriteria = " AND TransactionDate >= @StartDate";
			}
			else if (EndDate.HasValue)
			{
				filterCriteria = " AND TransactionDate <= @EndDate";
			}

			if (!string.IsNullOrEmpty(filterCriteria))
			{
				sqlBuilder.Append(filterCriteria);
			}
			sqlBuilder.Append(";"); // Safely close the COUNT query

			// 2. Build the dynamic SELECT query
			sqlBuilder.Append(@"
        SELECT Id, ReferenceId, Description, TransactionType, TransactionDate, Amount
        FROM UserTransactions 
        WHERE ReferenceId = @ReferenceId");

			if (!string.IsNullOrEmpty(filterCriteria))
			{
				sqlBuilder.Append(filterCriteria);
			}

			// 3. Complete with performance sorting and pagination limits
			sqlBuilder.Append(@"
        ORDER BY TransactionDate DESC, Id
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY;");

			int actualPageNumber = PageNumber ?? 1;
			int actualPageSize = PageSize ?? 10;
			var offset = (actualPageNumber - 1) * actualPageSize;

			using var multi = await _dbConnection.QueryMultipleAsync(sqlBuilder.ToString(), new
			{
				ReferenceId = referenceId,
				StartDate = StartDate,
				EndDate = EndDate,
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
