using MartenPostgreSqlService.Handlers;

namespace MartenPostgreSqlService.Endpoints
{
	public class CreateUserTransactionEndpoint :ICarterModule
	{
		public record CreateUserTransactionRequest(
		Guid Id,
		Guid ReferenceId,
		string Description,
		string TransactionType,
		DateTime TransactionDate,
		double Amount
		) : ICommand<CreateUserTransactionResponse>;

		public record CreateUserTransactionResponse(bool status);

		public void AddRoutes(IEndpointRouteBuilder app)
		{
			app.MapPost("/UserTransaction", async (CreateUserTransactionRequest request, ISender sender) =>
			{
				var command = request.Adapt<CreateUserTransactionCommand>();
				var result = await sender.Send(command);

				var response = result.Adapt<CreateUserTransactionResponse>();

				return Results.Created($"/UserTransaction/{response.status}", response);

			})
				.WithName("Creatd: Transaction")
				.Produces<CreateUserTransactionResponse>(StatusCodes.Status201Created)
				.ProducesProblem(StatusCodes.Status400BadRequest)
				.WithSummary("Creates a new user transaction")
				.WithDescription("Creates a new user transaction with the specified details.");
		}
	}
}
