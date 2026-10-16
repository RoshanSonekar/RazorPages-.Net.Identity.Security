using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Exceptions.Handler
{
	public class CustomExceptionHandler 
		(ILogger<CustomExceptionHandler> logger)
		: IExceptionHandler
	{
		public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
		{
			logger.LogError("Error MEssage : {ExceptionMessage}, time of occurrence {time}", exception.Message, DateTime.UtcNow);

			(string Detail, string Title, int StatusCode) details = exception switch
			{
				InternalServerException =>
				(
				exception.Message,
				exception.GetType().Name,
				context.Response.StatusCode = StatusCodes.Status500InternalServerError
				),
				ValidationException =>
				(
				exception.Message,
				exception.GetType().Name,
				context.Response.StatusCode = StatusCodes.Status400BadRequest
				),
				BadRequestException =>
				(
				exception.Message,
				exception.GetType().Name,
				context.Response.StatusCode = StatusCodes.Status400BadRequest
				),
				NotFoundException =>
				(
				exception.Message,
				exception.GetType().Name,
				context.Response.StatusCode = StatusCodes.Status500InternalServerError
				),
				_ =>
				(
				exception.Message,
				exception.GetType().Name,
				context.Response.StatusCode = StatusCodes.Status500InternalServerError
				)
			};

			var problemDetails = new ProblemDetails
			{
				Title = details.Title,
				Detail = details.Detail,
				Status = details.StatusCode,
				Instance = context.Request.Path
			};

			problemDetails.Extensions.Add("TracedId", context.TraceIdentifier);

			if (exception is ValidationException validationExecption)
			{
				problemDetails.Extensions.Add("ValidationError", validationExecption.Errors);
			}

			await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
			return true;

		}
	}
}
