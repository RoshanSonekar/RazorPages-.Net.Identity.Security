using AuthService.Data;
using AuthService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;
using HealthChecks.UI.Client;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

string connectionString = builder.Configuration.GetConnectionString("SqlServer") ?? throw new InvalidOperationException("Connection string 'SqlServer' not found.");
string jwtSecret = builder.Configuration["Jwt:Secret"]?? throw new InvalidOperationException("Secret not found to generate JWT token.");

IConfiguration configEnvironmentVariables = builder.Configuration;

if (builder.Configuration["Feature"] == "Global")
{
	#region --- Fetch Azure SQL DB Connectionstring. For Azure Deployment use 'Environment Variables' to get configuration for AzureVault 'url' and 'secret name'---
	string keyVaultUri = configEnvironmentVariables["KeyVaultURL"] ?? throw new InvalidOperationException("Key Vault URI not found.");
	var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
	KeyVaultSecret secret = await client.GetSecretAsync(configEnvironmentVariables["KeyVaultSecretName"] ?? throw new InvalidOperationException("Key Vault Secret Name not found."));
	connectionString = secret.Value ?? throw new InvalidOperationException("Secret value not found for KeyVaultSecretName.");

	secret = await client.GetSecretAsync(configEnvironmentVariables["JwtSecret"] ?? throw new InvalidOperationException("Key Vault Secret Name not found."));
	jwtSecret = secret.Value ?? throw new InvalidOperationException("Secret value not found JwtSecret.");
	#endregion
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseSqlServer(connectionString);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
		.AddEntityFrameworkStores<ApplicationDbContext>();

// JWT Authentication Setup
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key),
		ValidateIssuer = false,
		ValidateAudience = false
	};
});
builder.Services.AddAuthorization();

// Register Health Check Services
builder.Services.AddHealthChecks()
		.AddSqlServer(
				connectionString: connectionString,
				healthQuery: "SELECT 1;", // Default, but can be customized
				name: "sql-server",
				tags: new[] { "ready" }
		);


var app = builder.Build();

app.UseHealthChecks("/health",
	new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
	{
		ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
	});

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
