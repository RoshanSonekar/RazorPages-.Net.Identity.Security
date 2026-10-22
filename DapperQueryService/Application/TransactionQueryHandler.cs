using BuildingBlocks.CQRS;
using DapperQueryService.Core;
using DapperQueryService.Infra;
using System.Text.Json.Serialization;

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
public record GetTransactionsResult
{
	[JsonPropertyName("transactionEntities")]
	public IEnumerable<TransactionEntitySQL> TransactionEntities { get; init; }

	[JsonPropertyName("totalCount")]
	public int TotalCount { get; init; }

	public GetTransactionsResult(IEnumerable<TransactionEntitySQL> transactionEntities, int totalCount)
	{
		TransactionEntities = transactionEntities;
		TotalCount = totalCount;
	}
}
public record GetTransactionsQuery(Guid ReferenceId, int? PageNumber = 1, int? PageSize = 10) : IQuery<GetTransactionsResult>;

public class TransactionsQueryHandler(IUserTransactions userTranactionRepository)
	: IQueryHandler<GetTransactionsQuery, GetTransactionsResult>
{
	// Add cancellationToken if your repository method supports it
	public async Task<GetTransactionsResult> Handle(GetTransactionsQuery query, CancellationToken cancellationToken)
	{
		int pageNumber = query.PageNumber ?? 1;
		int pageSize = query.PageSize ?? 10;

		var (transactions, totalCount) = await userTranactionRepository.GetTransactionsAsync(
			query.ReferenceId,
			pageNumber,
			pageSize
		);

		return new GetTransactionsResult(transactions, totalCount);
	}
}	
#endregion
