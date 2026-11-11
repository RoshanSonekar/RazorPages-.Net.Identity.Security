using AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text; 
using System.Security.Cryptography;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IConfiguration _configuration;

	public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
	{
		_userManager = userManager;
		_configuration = configuration;
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] UserCredentialModel model)
	{
		var user = await _userManager.FindByEmailAsync(model.Email);

		if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
			return Unauthorized("Invalid credentials.");

		var authClaims = new List<Claim>
				{
						//new Claim(ClaimTypes.Name, user.UserName ?? ""),
						new Claim(ClaimTypes.NameIdentifier, user.Id),
						new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
				};

		var userRoles = await _userManager.GetRolesAsync(user);
		foreach (var role in userRoles)
		{
			authClaims.Add(new Claim(ClaimTypes.Role, role));
		}
		// 3. Fetch and add Custom Claims (from AspNetUserClaims)
		var userClaims = await _userManager.GetClaimsAsync(user);
		foreach (var claim in userClaims)
		{
			// 💡 This preserves whatever Type and Value are configured in your DB rows
			// e.g., Type: "Permission", Value: "CanDelete"
			authClaims.Add(new Claim(claim.Type, claim.Value));
		}

		// Generate tokens
		var accessToken = GenerateAccessToken(authClaims);
		var refreshToken = GenerateRefreshToken();

		// Save refresh token to database
		user.RefreshToken = refreshToken;
		user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Long-lived
		await _userManager.UpdateAsync(user);

		return Ok(new TokenResponse(accessToken, refreshToken));
	}

	[HttpPost("refresh")]
	public async Task<IActionResult> Refresh([FromBody] TokenRequestModel model)
	{
		if (model == null || string.IsNullOrEmpty(model.AccessToken) || string.IsNullOrEmpty(model.RefreshToken))
			return BadRequest("Invalid client request.");

		// 1. Extract claims from the EXPIRED access token
		var principal = GetPrincipalFromExpiredToken(model.AccessToken);
		if (principal == null)
			return BadRequest("Invalid access token or refresh token.");

		// 2. Locate user based on token claims
		var username = principal.Identity?.Name;
		var user = await _userManager.FindByNameAsync(username);

		// 3. Validate the refresh token against database records
		if (user == null || user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
			return BadRequest("Invalid access token or refresh token.");

		// 4. Token Rotation: Generate a brand-new pair
		var newAccessToken = GenerateAccessToken(principal.Claims);
		var newRefreshToken = GenerateRefreshToken();

		user.RefreshToken = newRefreshToken;
		user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
		await _userManager.UpdateAsync(user);

		return Ok(new TokenResponse(newAccessToken, newRefreshToken));
	}

	// --- HELPER METHODS ---

	private string GenerateAccessToken(IEnumerable<Claim> claims)
	{
		var jwtSecret = _configuration["Jwt:Secret"] ?? "A_Very_Long_And_Super_Secret_Key_64_Bytes!";
		var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Expires = DateTime.UtcNow.AddMinutes(15), // Short-lived access token
			SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
		};

		var tokenHandler = new JwtSecurityTokenHandler();
		var token = tokenHandler.CreateToken(tokenDescriptor);
		return tokenHandler.WriteToken(token);
	}

	private string GenerateRefreshToken()
	{
		var randomNumber = new byte[64];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomNumber);
		return Convert.ToBase64String(randomNumber);
	}

	private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
	{
		var jwtSecret = _configuration["Jwt:Secret"] ?? "A_Very_Long_And_Super_Secret_Key_64_Bytes!";
		var tokenValidationParameters = new TokenValidationParameters
		{
			ValidateAudience = false,
			ValidateIssuer = false,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
			ValidateLifetime = false // Critical: We want to read claims from an EXPIRED token
		};

		var tokenHandler = new JwtSecurityTokenHandler();
		var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

		if (securityToken is not JwtSecurityToken jwtSecurityToken ||
				!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
			return null;

		return principal;
	}
} 