using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using WebAppRazor_Security.Authorisation;
using WebAppRazor_Security.DTO;
using WebAppRazor_Security.Pages.Account;

namespace WebAppRazor_Security.Pages.HR;

[Authorize(Policy = "HRManagerOnly")]
public class HRManagerModel : PageModel
{
	private readonly IHttpClientFactory httpClientFactory;

	[BindProperty]
	public List<DTOWeatherForcast>? dTOWeatherForcastsItems { get; set; }
	public HRManagerModel(IHttpClientFactory _httpClientFactory)
	{
		httpClientFactory = _httpClientFactory;
	}
	public async Task OnGet()
	{
		// get token from session
		JwtToken token = new ();
		var strSessionTokenObj = HttpContext.Session.GetString("access_token");

		if (string.IsNullOrEmpty(strSessionTokenObj))
			token= await Authenticate(new Credential { Username = "admin", Password = "password" }); 
		else
			token = JsonConvert.DeserializeObject<JwtToken>(strSessionTokenObj)??new JwtToken();

		if (token is null || string.IsNullOrEmpty(token.AccessToken) || token.ExpiresAt <= DateTime.Now)
			token = await Authenticate(new Credential { Username = "admin", Password = "password" });

		var client = httpClientFactory.CreateClient("WebAPI");
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token?.AccessToken??string.Empty);
		dTOWeatherForcastsItems = await client.GetFromJsonAsync<List<DTOWeatherForcast>>("WeatherForecast");
	}

	private async Task<JwtToken> Authenticate(Credential credential)
	{
		var client = httpClientFactory.CreateClient("WebAPI");
		var res = await client.PostAsJsonAsync("auth", new Credential { Username = credential.Username, Password = credential.Password });
		res.EnsureSuccessStatusCode();
		string strToken = await res.Content.ReadAsStringAsync();

		HttpContext.Session.SetString("access_token",strToken);
		return JsonConvert.DeserializeObject<JwtToken>(strToken)?? new JwtToken();
	}
}
