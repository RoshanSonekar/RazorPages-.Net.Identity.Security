using UserProfileService.Models;

namespace UserProfileService.Services
{
	public interface IUserProfile
	{
		Task<bool> CreateUserProfileAsync(UserProfileModel userProfile);
		Task<bool> UpdateUserProfileAsync(UserProfileModel userProfile);

		Task<string> GetUserProfileAsync(Guid userId);
	}
}
