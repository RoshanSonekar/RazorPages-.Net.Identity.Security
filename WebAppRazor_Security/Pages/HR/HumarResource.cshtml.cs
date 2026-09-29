using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAppRazor_Security.DTO;

namespace WebAppRazor_Security.Pages.HR;

[Authorize(Policy = "MustBelongsToHR")]
public class HumarResourceModel : PageModel
{
	//private readonly IHttpClientFactory httpClientFactory;
	//public List<DTOWeatherForcast>? dTOWeatherForcastsItems { get; set; }
	//public HumarResourceModel(IHttpClientFactory _httpClientFactory)
	//{
	//	httpClientFactory = _httpClientFactory;
	//}

	public async Task OnGet()
	{
		//var client = httpClientFactory.CreateClient("WebAPI");
		//dTOWeatherForcastsItems = await client.GetFromJsonAsync<List<DTOWeatherForcast>>("WeatherForecast");
	}
}
