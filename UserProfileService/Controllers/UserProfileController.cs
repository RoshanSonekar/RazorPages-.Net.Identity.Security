using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Services;

namespace UserProfileService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserProfileController : ControllerBase
	{
		private readonly IUserProfile userProfile;
		public UserProfileController(IUserProfile _userProfile)
		{
			 userProfile = _userProfile;
		}

		[HttpPost]
		public async Task<IActionResult> CreateUserProfile([FromBody] Models.UserProfileModel user)
		{
			if (userProfile is null)
				return BadRequest("User profile data is required.");

			// Assuming '_userProfileService' is your injected dependency instance
			bool flag = await userProfile.CreateUserProfileAsync(user);

			if (flag)
			{
				// Return a 200 OK HTTP status code containing a JSON object response
				return Ok(new { success = true, message = "User profile created successfully." });
			}

			// Handle failure case (e.g., username taken, database error)
			return BadRequest(new { success = false, message = "Failed to create user profile." });
		}

		[HttpPut]
		public async Task<IActionResult> UpdateUserProfile([FromBody] Models.UserProfileModel user)
		{
			if (userProfile is null)
				return BadRequest("User profile data is required.");

			// Assuming '_userProfileService' is your injected dependency instance
			bool flag = await userProfile.UpdateUserProfileAsync(user);

			if (flag)
			{
				// Return a 200 OK HTTP status code containing a JSON object response
				return Ok(new { success = true, message = "User profile updated successfully." });
			}

			// Handle failure case (e.g., username taken, database error)
			return BadRequest(new { success = false, message = "Failed to update user profile. Kindly check the request and mandatory fields. Ex- Id must be exist to update the record."  });
		}

		[HttpGet("{userId:guid}")]
		public async Task<IActionResult> GetUserProfilePayload([FromRoute] Guid userId)
		{
			if (userId == Guid.Empty)
				return BadRequest(new { message = "A valid User ID is required." });

			string? payloadJson = await userProfile.GetUserProfileAsync(userId);

			if (string.IsNullOrEmpty(payloadJson))
				return NotFound(new { message = $"User profile with ID {userId} was not found." });

			// Return the payload string with a 200 OK status
			return Ok(payloadJson);
		}
	}
}
