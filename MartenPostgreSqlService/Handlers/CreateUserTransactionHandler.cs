namespace MartenPostgreSqlService.Handlers
{
	public record CreateUserTransactionCommand(
		Guid Id,
		Guid ReferenceId,
		string Description,
		string TransactionType,
		DateTime TransactionDate,
		double Amount
	) : ICommand<CreateUserTransactionResult>;

	public record CreateUserTransactionResult(bool status);

	internal class CreateUserTransactionHandler	(IDocumentSession session)
		: ICommandHandler<CreateUserTransactionCommand, CreateUserTransactionResult>
	{
		public async Task<CreateUserTransactionResult> Handle(CreateUserTransactionCommand command, CancellationToken cancellationToken)
		{
			// create product entity from command
			var userTransaction = new Models.UserTransactionPostgreSqlModel
			{
				Id = command.Id,
				ReferenceId = command.ReferenceId,
				Description = command.Description,
				TransactionType = command.TransactionType,
				TransactionDate = command.TransactionDate,
				Amount = command.Amount
			};

			// save to PostgreSql 
			session.Store(userTransaction);
			await session.SaveChangesAsync(cancellationToken);

			// return createProductResult result
			return new CreateUserTransactionResult(true);
		}
	}
}
