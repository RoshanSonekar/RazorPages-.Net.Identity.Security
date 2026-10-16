using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BuildingBlocks.Behaviors
{
	public class LoggingBehavior<TRequest, TResponse>
		(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
		: IPipelineBehavior<TRequest, TResponse>
		where TRequest : notnull, IRequest<TResponse>
		where TResponse : notnull
	{
		public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
		{
			logger.LogInformation("[START] handle Request={Request} - Response={Response} - RequestData={RequestData}"
				, typeof(TRequest).Name, typeof(TResponse).Name, request);

			var timer = new Stopwatch();
			timer.Start();

			var response = await next();

			timer.Stop();
			var timeTaken = timer.Elapsed;
			// read set threshold from config. If request is taking greater than 3(threshold value) sec then log performance
			if (timeTaken.Seconds > 3) 
				logger.LogWarning("[PERFORMANCE] The request {Request} took {TImeTaken}",
					typeof(TRequest).Name, timeTaken.Seconds);

			logger.LogInformation("[END] handled {Request} with {Response}", typeof(TRequest).Name, typeof(TResponse).Name);
			return response;
		}
	}
}