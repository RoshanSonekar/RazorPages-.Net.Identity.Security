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
					DateOfBirth = userProfile.DateOfBirth,
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
				DtoUserProfile DTO = new()
				{
					Id = userProfile.Id,
					FirstName = userProfile.FirstName,
					LastName = userProfile.LastName,
					MobileNumber = userProfile.MobileNumber,
					DateOfBirth = userProfile.DateOfBirth,
					Payload = JsonSerializer.Serialize(userProfile)
				};
				dbcontext.Add(DTO);

				int rowsAffected = await dbcontext.DtoUserProfile
			 .Where(u => u.Id == userProfile.Id)
			 .ExecuteUpdateAsync(setters => setters
					 .SetProperty(u => u.FirstName, u=> string.IsNullOrWhiteSpace(DTO.FirstName) ? u.FirstName: DTO.FirstName)
					 .SetProperty(u => u.LastName, u => string.IsNullOrWhiteSpace(DTO.LastName) ? u.LastName : DTO.LastName)
					 .SetProperty(u => u.DateOfBirth, DTO.DateOfBirth)
					 .SetProperty(u => u.MobileNumber, u => string.IsNullOrWhiteSpace(DTO.MobileNumber) ? u.MobileNumber : DTO.MobileNumber)
					 .SetProperty(u => u.Payload, JsonSerializer.Serialize(DTO))
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
			//var userDetails = await dbcontext.DtoUserProfile.FirstOrDefaultAsync(x => x.Id == userId);
			var userDetails = await dbcontext.DtoUserProfile
			.Where(x => x.Id == userId)
			.Select(x => new
			{
				x.FirstName,
				x.LastName,
				x.DateOfBirth,
				x.MobileNumber,
				x.Id 
			})
		.FirstOrDefaultAsync();

			if (userDetails == null)
				return string.Empty;

			// Configure JSON options to match standard camelCase formatting
			//var options = new JsonSerializerOptions
			//{
			//	PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			//	WriteIndented = false
			//};

			// Converts the anonymous object into a string to match Task<string>
			return JsonSerializer.Serialize(userDetails);
		}
	}
}
