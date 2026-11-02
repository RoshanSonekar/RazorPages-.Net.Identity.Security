namespace WebApp.Integration.Email
{
	public interface IEmailService
	{
		Task Send(EmailConfirmationModelRequest emailConfirmationModelRequest); 
	}
	public interface IEmailServiceV2
	{
		ValueTask<HttpClient> CreateHttpClientContextAsync(HttpClient httpClient, EmailConfirmationModelRequest request);
		Task<EmailConfirmationModelResponse> Send(EmailConfirmationModelRequest emailConfirmationModelRequest);
	}
}
