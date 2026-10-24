using MediatR;
using Microsoft.AspNetCore.Mvc;
using DapperQueryService.Application;

namespace DapperQueryService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserTransactionSqlController : ControllerBase
	{
		//private readonly ILogger<BookingController> _logger;
		private readonly IMediator _mediatR;	

		public UserTransactionSqlController(IMediator mediatR)
		{
			_mediatR = mediatR;
		}

		// for Add transaction
		[HttpPost]
		public async Task<IActionResult> CreateUserTransaction([FromBody] AddTranactionCommand command)
		{
			var result = await _mediatR.Send(command);
			return result.IsSuccess ? Ok(result) : BadRequest(result);
		}

		// get trnsaction by transaction id
		[HttpGet("{transaction_id}")]
		public async Task<IActionResult> GetUserTransaction(Guid transaction_id)
		{
			var trans = await _mediatR.Send(new GetTransactionQuery(transaction_id));

			if (trans.transactionEntity.Id == Guid.Empty)
				return NotFound(new
				{
					Error = "TransactionNotFound",
					Message = $"Transaction with id {transaction_id} was not found."
				});

			return Ok(trans);
		}
	}
}
