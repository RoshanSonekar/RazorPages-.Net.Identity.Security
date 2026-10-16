using BuildingBlocks.CQRS;
using DapperQueryService.Core;
using DapperQueryService.Infra;
using FluentValidation; 

namespace DapperQueryService.Application;

public record AddTransactionResult(Guid Id, bool IsSuccess);
public record AddTranactionCommand(Guid ReferenceId, string Description, string TransactionType, DateTime TransactionDate, double Amount) 
: ICommand<AddTransactionResult>;

public class AddTranactionCommandValidator : AbstractValidator<AddTranactionCommand>
{
	public AddTranactionCommandValidator()
	{
		RuleFor(x => x.ReferenceId).NotEmpty().WithMessage("Flight number is required.");
		RuleFor(x => x.TransactionType).NotEmpty().WithMessage("Origin is required.");
		RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
		RuleFor(x => x.Amount).LessThan(99999).WithMessage("Amount must be less than 99999.");
		RuleFor(x => x.TransactionDate).LessThan(DateTime.UtcNow).WithMessage("Departure time must not be in the past.");
	}
}

public class TransactionCommandHandler(IUserTransactions userTranactionRepository)
	: ICommandHandler<AddTranactionCommand, AddTransactionResult>
{
	public async Task<AddTransactionResult> Handle(AddTranactionCommand command, CancellationToken cancellationToken)
	{
		var transaction = new TransactionEntitySQL
		{
			Id = Guid.NewGuid(),
			ReferenceId = command.ReferenceId,
			Description = command.Description,
			TransactionType = command.TransactionType,
			TransactionDate = command.TransactionDate,
			Amount = command.Amount
		};
		await userTranactionRepository.AddTransactionAsync(transaction);
		return new AddTransactionResult(transaction.Id, true);
	}
}
 
