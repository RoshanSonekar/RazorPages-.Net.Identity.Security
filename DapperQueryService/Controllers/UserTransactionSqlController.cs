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

		// for Add flight
		[HttpPost]
		public async Task<IActionResult> CreateUserTransaction([FromBody] AddTranactionCommand command)
		{
			var result = await _mediatR.Send(command);

			return result.IsSuccess ? Ok(result) : BadRequest(result);

			//return CreatedAtAction(nameof(GetUserTransactionById), new { id = result.Id }, result);
		}

		// get flight by id
		[HttpGet]
		public async Task<IActionResult> GetUserTransactions(Guid id)
		{
			var trans = await _mediatR.Send(new GetTransactionsQuery(1, 10));

			if (trans is null)
				return NotFound(new
				{
					Error = "TransactionNotFound",
					Message = $"Something went wrong. Please try again later."
				});

			return Ok(trans);
		}

		// get flight by id
		[HttpGet("{id}")]
		public async Task<IActionResult> GetUserTransactionById(Guid id)
		{
			var trans = await _mediatR.Send(new GetTransactionQuery(id));

			if (trans.transactionEntity.Id == Guid.Empty)
				return NotFound(new
				{
					Error = "TransactionNotFound",
					Message = $"Transaction with id {id} was not found."
				});

			return Ok(trans);
		}
	}
}
