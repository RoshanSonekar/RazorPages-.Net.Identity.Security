using BuildingBlocks.CQRS;
using DapperQueryService.Core;
using DapperQueryService.Infra;

namespace DapperQueryService.Application;

#region ---- Query to get specific transaction by Id ----
public record GetTransactionResult(TransactionEntitySQL transactionEntity);
public record GetTransactionQuery(Guid Id) : IQuery<GetTransactionResult>;

public class TransactionQueryHandler(IUserTransactions transactionRepository)
	: IQueryHandler<GetTransactionQuery, GetTransactionResult>
{
	public async Task<GetTransactionResult> Handle(GetTransactionQuery query, CancellationToken cancellationToken)
	{
		var transaction = await transactionRepository.GetTransactionByIdAsync(query.Id); // Add cancellationToken if your repository method supports it
		return new GetTransactionResult(transaction);
	}
}
#endregion

#region ---- Query to get all user's transactions with pagination ----
public record GetTransactionsResult(IEnumerable<TransactionEntitySQL> transactionEntities);
public record GetTransactionsQuery(Guid ReferenceId, int? PageNumber = 1, int? PageSize = 10) : IQuery<GetTransactionsResult>; // get paginations valus config or UI

public class TransactionsQueryHandler(IUserTransactions userTranactionRepository)
	: IQueryHandler<GetTransactionsQuery, GetTransactionsResult>
{
	// Add cancellationToken if your repository method supports it
	public async Task<GetTransactionsResult> Handle(GetTransactionsQuery query, CancellationToken cancellationToken)
	{
		var transactions = await userTranactionRepository.GetTransactionsAsync(query.PageNumber ?? 1, query.PageSize ?? 10);
		return new GetTransactionsResult(transactions);
	}
}
#endregion
