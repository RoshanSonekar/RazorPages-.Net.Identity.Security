using DapperQueryService.Core;

namespace DapperQueryService.Infra
{
	public interface IUserTransactions
	{
		Task<bool> AddTransactionAsync(TransactionEntitySQL transactionEntitySQL);
		Task<TransactionEntitySQL> GetTransactionByIdAsync(Guid id);
		Task<(IEnumerable<TransactionEntitySQL> Transactions, int TotalCount)> GetTransactionsAsync(Guid referenceId, int? PageNumber = 1, int? PageSize = 10);
		Task<(IEnumerable<TransactionEntitySQL> Transactions, int TotalCount)> GetTransactionsByFilterAsync(Guid referenceId, DateTime? StartDate = null, DateTime? EndDate = null, int? PageNumber = 1, int? PageSize = 10);
		Task<IEnumerable<TransactionEntitySQL>> GetTransactionsAsyncOld(Guid referenceId, int? PageNumber = 1, int? PageSize = 10);
	}
	 
}
