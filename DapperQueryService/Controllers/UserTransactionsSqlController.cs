using DapperQueryService.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DapperSqlService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserTransactionsSqlController : ControllerBase
	{//private readonly ILogger<BookingController> _logger;
		private readonly IMediator _mediatR;

		public UserTransactionsSqlController(IMediator mediatR)
		{
			_mediatR = mediatR;
		}

		// get transactions by ref/user id
		[HttpGet("{referenceId}")]
		public async Task<IActionResult> GetUserTransactions(Guid referenceId, [FromQuery] int? PageNumber = 1, [FromQuery] int? PageSize = 10, CancellationToken cancellationToken = default)
		{
			var result = await _mediatR.Send(new GetTransactionsQuery(referenceId, PageNumber, PageSize), cancellationToken);

			if (result is null || result.TransactionEntities is null)
			{
				return NotFound(new
				{
					Error = "TransactionsNotFound",
					Message = "Something went wrong. Please try again later."
				});
			}

			return Ok(new
			{
				transactionEntities = result.TransactionEntities,
				totalCount = result.TotalCount
			}); 
		}
	}
}
