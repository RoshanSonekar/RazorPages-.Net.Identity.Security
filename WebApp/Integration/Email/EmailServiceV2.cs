//using System.ComponentModel.DataAnnotations;
//using System.Net;
//using System.Text.Json;
//using WebApp.Integration.Email; 

//namespace WebAppAuthentication.Integration.Email
//{
//	public class EmailServiceV2 : IEmailServiceV2
//	{
//		private readonly IHttpClientFactory httpClientFactory;
//		private readonly ILogger<EmailService> logger;
//		private readonly IConfiguration configuration;

//		public EmailServiceV2(IHttpClientFactory httpClientFactory,
//											 ILogger<EmailService> logger,
//											 IConfiguration configuration)
//		{
//			this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
//			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
//			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
//		}

//		public async Task<EmailConfirmationModelResponse> Send(EmailConfirmationModelRequest emailConfirmationModelRequest)
//		{
//			if (emailConfirmationModelRequest is null || string.IsNullOrWhiteSpace(emailConfirmationModelRequest.Email))
//			{
//				logger.LogWarning("Email address cannot be null or empty");
//				throw new ArgumentException("Email address must be provided and not empty", nameof(emailConfirmationModelRequest));
//			}

//			var client = CreateClient();

//			using (var clientContext = await CreateHttpClientContextAsync(client, emailConfirmationModelRequest))
//			{
//				try
//				{
//					logger.LogDebug($"Sending confirmation email to {emailConfirmationModelRequest.Email}");

//					var response = await client.PostAsJsonAsync("SendAccountConfirmationEmail",
//																											emailConfirmationModelRequest);

//					response.EnsureSuccessStatusCode();

//					return await GetHttpResponseData(clientContext, response).ConfigureAwait(false);
//				}
//				catch (Exception ex) when (!emailValidationExceptions(ex))
//				{
//					logger.LogError(ex, "Failed to send confirmation email to {Email}", emailConfirmationModelRequest.Email);
//					throw;
//				}
//			}
//		}

//		private static bool emailValidationExceptions(Exception ex) =>
//				ex.GetType().FullName?.StartsWith("System.Net.HttpStatusCodeException") ?? false;

//		private HttpClient CreateClient()
//		{
//			var client = httpClientFactory.CreateClient("EmailNotificationAPI");
//			ConfigureClient(client);
//			return client;
//		}

//		public async ValueTask<HttpClient> CreateHttpClientContextAsync(HttpClient httpClient,
//																																					EmailConfirmationModelRequest1 request)
//		{
//			var cancellationTokenSource = CancellationToken.None;

//			// Add timeout and cancellation logic
//			if (httpClientFactory != null && configuration != null)
//			{
//				var timeoutMs = TryGetConfigurationTimeout(configuration, "EmailApiTimeout");
//				httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
//			}

//			return await httpClient.SendAsync(cancellationTokenSource, request).ConfigureAwait(false);
//		}

//		private async Task<EmailConfirmationModelResponse> GetHttpResponseData(
//				HttpClient clientContext,
//				HttpResponseMessage response)
//		{
//			var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

//			// Parse the response to EmailConfirmationModelResponse
//			return JsonSerializer.Deserialize<EmailConfirmationModelResponse>(responseContent);
//		}

//		private void ConfigureClient(HttpClient client)
//		{
//			// Add headers and configuration
//			client.DefaultRequestHeaders.Add("Authorization", configuration["Jwt:Secret"]);
//			client.DefaultRequestHeaders.Add("X-Custom-Header", GetConfiguredValue(configuration, "CustomApiKey"));

//			// Set default timeout if not already set
//			client.Timeout = TimeSpan.FromSeconds(60);
//		}

//		private static int TryGetConfigurationTimeout(IConfiguration configuration, string key)
//		{
//			return int.TryParse(configuration[key], out var result) ? result : 30;
//		}

//		private static string GetConfiguredValue(IConfiguration configuration, string key)
//		{
//			return configuration.GetValue<string>(key);
//		}
//	}

//	public class EmailConfirmationModelRequest1
//	{
//		[Required]
//		[Display(Name = "Email")]
//		[EmailAddress(ErrorMessage = "Invalid email address.")]
//		public string Email { get; set; } = string.Empty;
//		public string Subject { get; set; } = string.Empty;
//		public string MessageBody { get; set; } = string.Empty;
//	}
//}