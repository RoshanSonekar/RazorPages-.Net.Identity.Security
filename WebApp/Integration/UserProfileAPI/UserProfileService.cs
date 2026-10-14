using Azure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using WebApp.Data.Account;

namespace WebAppAuthentication.Integration.UserProfileAPI
{
	public class UserProfileService : IUserProfileService
	{
		private readonly IHttpClientFactory httpClientFactory;

		public UserProfileService(IHttpClientFactory _httpClientFactory)
		{
			httpClientFactory = _httpClientFactory;
		}

		public async Task<bool> UpsertUserProfileAsync(UserProfile userProfile, bool IsProfileExist)
		{
			if (userProfile is null)
				return false;

			var client = httpClientFactory.CreateClient("UserProfileService");
			if (!IsProfileExist)
			{
				var response = await client.PostAsJsonAsync("UserProfile", userProfile);
				response.EnsureSuccessStatusCode();
			}
			else
			{
				var response = await client.PutAsJsonAsync("UserProfile", userProfile);
				response.EnsureSuccessStatusCode();
			}
			return IsProfileExist;
		}
		 

		public async Task<IActionResult> GetUserProfilePayload(Guid userId)
		{
			var client = httpClientFactory.CreateClient("UserProfileService");
			string requestUri = $"UserProfile/{userId}";

			using HttpResponseMessage response = await client.GetAsync(requestUri);

			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				var errorBody = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
				string message = errorBody?.GetValueOrDefault("message") ?? "User not found.";

				return new JsonResult(new { message })
				{
					StatusCode = StatusCodes.Status404NotFound
				};
			}			 
			else if (!response.IsSuccessStatusCode)
			{
				var errorBody = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
				string message = errorBody?.GetValueOrDefault("message") ?? "The downstream profile service returned an error.";

				return new JsonResult(new { message })
				{
					StatusCode = StatusCodes.Status500InternalServerError
				};
			}

			string content = await response.Content.ReadAsStringAsync();
			return new JsonResult(content)
			{
				StatusCode = StatusCodes.Status200OK
			};
		}
	}
}
