using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UserProfileService.Data;
using UserProfileService.Services;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// Add services to the container.

builder.Services.AddControllers();

string connectionString = builder.Configuration.GetConnectionString("SqlServer") ?? throw new InvalidOperationException("Connection string 'SqlServer' not found.");
IConfiguration configEnvironmentVariables = builder.Configuration;

if (builder.Configuration["Feature"] == "Global")
{
	#region --- Fetch Azure SQL DB Connectionstring. For Azure Deployment use 'Environment Variables' to get configuration for AzureVault 'url' and 'secret name'---
	string keyVaultUri = configEnvironmentVariables["KeyVaultURL"] ?? throw new InvalidOperationException("Key Vault URI not found.");
	var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
	KeyVaultSecret secret = await client.GetSecretAsync(configEnvironmentVariables["KeyVaultSecretName"] ?? throw new InvalidOperationException("Key Vault Secret Name not found."));
	connectionString = secret.Value ?? throw new InvalidOperationException("Secret value not found.");
	#endregion
}

// Add db context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseSqlServer(connectionString));

// Application Services
builder.Services.AddScoped<IUserProfile, UserProfile>();

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

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
