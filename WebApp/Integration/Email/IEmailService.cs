namespace WebApp.Integration.Email
{
	public interface IEmailService
	{
		Task Send(EmailConfirmationModelRequest emailConfirmationModelRequest);
	}
}
