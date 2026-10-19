using WebAppAuthentication.Data.Entities;

namespace WebAppAuthentication.Integration.Graphql
{
	public class GraphqlQueryService : IGraphqlQueryService
	{
		public GraphqlQueryService()
		{
			
		}
		public async Task<IQueryable<UserTransactionEntity>> GetUserTransaction(Guid ReferenceId, DateOnly FromDate, DateOnly ToDate, string TransactionType)
		{
			throw new NotImplementedException();
		}
	}
}
