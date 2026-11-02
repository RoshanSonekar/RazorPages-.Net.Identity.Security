using MediatR;
using Microsoft.AspNetCore.Mvc;
using DapperQueryService.Application;
using System.Net;

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

		// to add user transaction
		[HttpPost]
		public async Task<IActionResult> CreateUserTransaction([FromBody] AddTranactionCommand command)
		{
			try
			{
				var result = await _mediatR.Send(command);
				return StatusCode(StatusCodes.Status201Created, result.Id);
			}
			catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
					when (ex.InnerException?.Message.Contains("duplicate") == true)
			{
				return Conflict(new { message = "A transaction with this ReferenceId already exists." });
			}
			catch (System.ComponentModel.DataAnnotations.ValidationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected server error occurred." });
			}
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
