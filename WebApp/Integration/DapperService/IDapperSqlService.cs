using Microsoft.AspNetCore.Mvc;
using WebAppAuthentication.Data.Entities;

namespace WebAppAuthentication.Integration.DapperService
{
	public interface IDapperSqlService
	{
		Task<List<UserTransactionEntity>> GetUserTransactions(Guid ReferenceId, int? PageNumber = 1, int? PageSize = 10);
		Task<IActionResult> GetUserTransaction(Guid TransactionId);
		Task<PagedApiResponse> GetUserTransactionsWithMetadata(Guid ReferenceId, int? PageNumber = 1, int? PageSize = 10);
	}
}
