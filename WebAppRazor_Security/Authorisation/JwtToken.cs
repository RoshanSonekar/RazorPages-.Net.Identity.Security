using Newtonsoft.Json;

namespace WebAppRazor_Security.Authorisation
{
	public class JwtToken
	{
		[JsonProperty("access_token")]
		public string AccessToken { get; set; } = string.Empty;
		[JsonProperty("expires_at")]
		public DateTime  ExpiresAt { get; set; }
	}
}
