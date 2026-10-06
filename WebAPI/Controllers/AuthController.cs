using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace WebAPI.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IConfiguration configuration;
		public AuthController(IConfiguration _configuration)
		{
			configuration= _configuration;
		}

		[HttpPost]
		public IActionResult Authenticate([FromBody] Credential credential)
		{
			if (credential is null)
				throw new ArgumentNullException(nameof(credential));
			if (string.IsNullOrWhiteSpace(credential.UserName) || string.IsNullOrWhiteSpace(credential.Password))
				throw new ArgumentException("Provide username and password");

			// verify credentials
			if (credential.UserName == "admin" && credential.Password == "password")
			{
				// create security context
				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, "admin"),
					new Claim(ClaimTypes.Email, "admin@gmail.com"),
					new Claim("Admin","true"),
					new Claim("Manager","true"),
					new Claim("Department","HR"), // custom claim for authorisation policy >> to get from DB
					new Claim("EmploymentDate","2025-09-09"),
				};

				var expireAt = DateTime.UtcNow.AddMinutes(2);
				return Ok(new
				{
					access_token = CreateToken(claims, expireAt),
					expired_At = expireAt

				});
			}

			ModelState.AddModelError("Unauthorized", "You are not authorized to access this endpoint");
			var problemDetails = new ProblemDetails
			{
				Title = "Unauthorized",
				Status = StatusCodes.Status401Unauthorized
			};
			return Unauthorized(problemDetails);

		}

		private string CreateToken(List<Claim> claims, DateTime expireAt)
		{
			var claimDictionary =  new Dictionary<string, object>();
			if (claims is not null && claims.Count > 0)
			{
				foreach (var claim in claims)
					claimDictionary.Add(claim.Type, claim.Value);

			}

			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Claims = claimDictionary,
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(
					System.Text.Encoding.UTF8.GetBytes(configuration["SecretKey"]??string.Empty)), 
					SecurityAlgorithms.HmacSha256Signature),
				Expires = expireAt,

			};
			var tokenHandler = new JsonWebTokenHandler();
			return tokenHandler.CreateToken(tokenDescriptor);

		}
	}

	public class Credential
	{
		[Required]
		public string UserName { get; set; } = string.Empty;

		[Required]
		public string Password { get; set; } = string.Empty;
	}
}
