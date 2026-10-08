using System.ComponentModel.DataAnnotations;

namespace WebApp.Integration.Email;

public class EmailService : IEmailService
{
	private readonly IHttpClientFactory httpClientFactory;

	public EmailService(IHttpClientFactory _httpClientFactory)
	{
		httpClientFactory = _httpClientFactory;
	}

	public async Task Send(EmailConfirmationModelRequest emailConfirmationModelRequest)
	{
		if (emailConfirmationModelRequest is null)
		{
			return;
		}

		var client = httpClientFactory.CreateClient("EmailNotificationAPI");
		var response = await client.PostAsJsonAsync("SendAccountConfirmationEmail", emailConfirmationModelRequest);

		// This throws an exception if the HTTP status code indicates failure (e.g., 404, 500)
		response.EnsureSuccessStatusCode();
	}
}

public class EmailConfirmationModelRequest
{
	[Required]
	[Display(Name = "Email")]
	[EmailAddress(ErrorMessage = "Invalid email address.")]
	public string Email { get; set; } = string.Empty;
	public string Subject { get; set; } = string.Empty;
	public string MessageBody { get; set; } = string.Empty;
}

public class EmailConfirmationModelResponse
{
	public string Email { get; set; } = string.Empty;
	public string MessageId { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
}
