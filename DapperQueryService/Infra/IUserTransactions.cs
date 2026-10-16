using DapperQueryService.Core;

namespace DapperQueryService.Infra
{
	public interface IUserTransactions
	{
		Task<IEnumerable<TransactionEntitySQL>> GetTransactionsAsync(int? PageNumber = 1, int? PageSize = 10);
		Task<bool> AddTransactionAsync(TransactionEntitySQL flight);
		Task<TransactionEntitySQL> GetTransactionByIdAsync(Guid id);
	}
}
