using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApp.Data.Account;

namespace WebAppAuthentication.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[AllowAnonymous]
	public class AccountController : ControllerBase
	{
		private readonly SignInManager<User> signInManager;

		public AccountController(SignInManager<User> _signInManager)
		{
			signInManager = _signInManager;
		}

		// FIXED: Re-enabled explicit routing attributes for GET handshakes
		[HttpGet("ExternalLoginCallback")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> ExternalLoginCallback()
		{
			// Read cookie authentication packet returned from signin-facebook
			var loginInfo = await signInManager.GetExternalLoginInfoAsync();
			if (loginInfo is null)
			{
				return BadRequest("Error loading external login information.");
			}

			var emailClaim = loginInfo.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
			var userNameClaim = loginInfo.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);

			if (userNameClaim is not null && emailClaim is not null)
			{
				// Ensure the user exists in SQL Server before attempting to sign them in
				var user = await signInManager.UserManager.FindByEmailAsync(emailClaim.Value);
				if (user is null)
				{
					user = new User
					{
						UserName = emailClaim.Value,
						Email = emailClaim.Value,
						EmailConfirmed = true // Confirm email so the login options constraints don't block them
					};
					await signInManager.UserManager.CreateAsync(user);
				}

				await signInManager.SignInAsync(user, isPersistent: false);
			}

			return Redirect("/Index");
		}

		[HttpGet("ExternalLoginCallbackNew")]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> ExternalLoginCallbackNew()
		{
			// 1. Read the external login information from the external cookie
			var loginInfo = await signInManager.GetExternalLoginInfoAsync();
			if (loginInfo is null)
			{
				return BadRequest("Error loading external login information.");
			}

			// 2. Safely extract the required email claim
			var emailClaim = loginInfo.Principal.FindFirst(ClaimTypes.Email);
			if (emailClaim is null || string.IsNullOrWhiteSpace(emailClaim.Value))
			{
				return BadRequest("External provider did not return a valid email address.");
			}

			var email = emailClaim.Value;

			// 3. Check if a local user already exists with this email
			var user = await signInManager.UserManager.FindByEmailAsync(email);
			if (user is null)
			{
				user = new User
				{
					UserName = email,
					Email = email,
					EmailConfirmed = true,
					FirstName = "None",
					LastName = "Node",
					Department ="None",
					Designation = "None",
					DateOfBirth = DateTime.UtcNow.AddDays(-5000)
				};

				try
				{
					// 4. Create the local user account
					var createResult = await signInManager.UserManager.CreateAsync(user);
					if (!createResult.Succeeded)
					{
						// Log errors here (e.g., password complexity, duplicate usernames if configured differently)
						return BadRequest("Failed to create a local user account.");
					}
				}
				catch (DbUpdateException ex)
				{
					var sqlEx = ex.InnerException as SqlException;
					// Log sqlEx?.Message
					foreach (var entry in ex.Entries)
					{
						// entry.Entity gives the entity type (e.g., User)
						foreach (var prop in entry.CurrentValues.Properties)
						{
							if (prop.ClrType == typeof(DateTime) || prop.ClrType == typeof(DateTime?))
							{
								var val = entry.CurrentValues[prop.Name];
								// Log prop.Name and val
							}
						}
					}
					throw;
				}
			}

			// 5. Link the external provider (Facebook) to this local account if not already linked
			var userLogins = await signInManager.UserManager.GetLoginsAsync(user);
			if (!userLogins.Any(l => l.LoginProvider == loginInfo.LoginProvider && l.ProviderKey == loginInfo.ProviderKey))
			{
				var addLoginResult = await signInManager.UserManager.AddLoginAsync(user, loginInfo);
				if (!addLoginResult.Succeeded)
				{
					return BadRequest("Failed to link the external provider to the local account.");
				}
			}

			// 6. Sign the user into the local application cookie session
			await signInManager.SignInAsync(user, isPersistent: false, loginInfo.LoginProvider);

			return Redirect("/Index");
		}
	}
}