using Azure.Core.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UserProfileService.Data;
using UserProfileService.Data.DTO;
using UserProfileService.Models;

namespace UserProfileService.Services
{
	public class UserProfile : IUserProfile
	{
		private readonly ApplicationDbContext dbcontext;
		public UserProfile(ApplicationDbContext _dbcontext)//, ILogger<UserProfileModel> logger)
		{
			dbcontext = _dbcontext;
		}

		// Create
		public async Task<bool> CreateUserProfileAsync(UserProfileModel userProfile)
		{
			try
			{
				DtoUserProfile DtouserProfile = new()
				{
					Id = userProfile.Id,
					FirstName = userProfile.FirstName,
					LastName = userProfile.LastName,
					MobileNumber = userProfile.MobileNumber,
					Payload = JsonSerializer.Serialize(userProfile)
				};
				dbcontext.Add(DtouserProfile);

				await dbcontext.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{ 
				throw new Exception("CreateUserProfileAsync", ex);
			}
		}

		// Update
		public async Task<bool> UpdateUserProfileAsync(UserProfileModel userProfile)
		{
			try
			{
				DtoUserProfile DtouserProfile = new()
				{
					Id = userProfile.Id,
					FirstName = userProfile.FirstName,
					LastName = userProfile.LastName,
					MobileNumber = userProfile.MobileNumber,
					Payload = JsonSerializer.Serialize(userProfile)
				};
				dbcontext.Add(DtouserProfile);

				int rowsAffected = await dbcontext.DtoUserProfile
			 .Where(u => u.Id == userProfile.Id)
			 .ExecuteUpdateAsync(setters => setters
					 .SetProperty(u => u.FirstName, u=> string.IsNullOrWhiteSpace(userProfile.FirstName) ? u.FirstName: userProfile.FirstName)
					 .SetProperty(u => u.LastName, u => string.IsNullOrWhiteSpace(userProfile.LastName) ? u.LastName : userProfile.LastName)  
					 .SetProperty(u => u.MobileNumber, u => string.IsNullOrWhiteSpace(userProfile.MobileNumber) ? u.MobileNumber : userProfile.MobileNumber)
					 .SetProperty(u => u.Payload, JsonSerializer.Serialize(userProfile))
					 ); 

				return rowsAffected > 0;
			}
			catch (Exception ex)
			{
				throw new Exception("UpdateUserProfileAsync", ex);
			}
		}

		public async Task<string> GetUserProfileAsync(Guid userId)
		{
			var userDetails = await dbcontext.DtoUserProfile.FirstOrDefaultAsync(x => x.Id == userId);
			if (userDetails == null)
				return string.Empty;
			
			return userDetails.Payload;
		}
	}
}
