using Microsoft.AspNetCore.Mvc;
using WebApp.Data.Account;

namespace WebAppAuthentication.Integration.UserProfileAPI
{
	public interface IUserProfileService
	{
		Task<bool> UpsertUserProfileAsync(UserProfile  userProfile, bool IsProfileExist);

		Task<IActionResult> GetUserProfile(Guid userId);
	}
} 