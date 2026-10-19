using WebAppAuthentication.Data.Entities;

namespace WebAppAuthentication.Integration.Graphql
{
	public interface IGraphqlQueryService
	{
		Task<IQueryable<UserTransactionEntity>> GetUserTransaction(Guid ReferenceId, DateOnly FromDate, DateOnly ToDate, string TransactionType);
	}
}
