using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Account;
using WebApp.Integration.Email;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
string connectionString =  builder.Configuration.GetConnectionString("SqlServer") ?? throw new InvalidOperationException("Connection string 'SqlServer' not found.");
IConfiguration configEnvironmentVariables = builder.Configuration;

if (builder.Configuration["Feature"] == "global")
{
	#region --- Fetch Azure SQL DB Connectionstring. For Azure Deployment use 'Environment Variables' to get configuration for AzureVault 'url' and 'secret name'---
	string keyVaultUri = configEnvironmentVariables["KeyVaultURL"] ?? throw new InvalidOperationException("Key Vault URI not found.");
	var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
	KeyVaultSecret secret = await client.GetSecretAsync(configEnvironmentVariables["KeyVaultSecretName"] ?? throw new InvalidOperationException("Key Vault Secret Name not found."));
	connectionString = secret.Value ?? throw new InvalidOperationException("Secret value not found.");
	#endregion
}


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{ 
	options.UseSqlServer(connectionString);
	//options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")); // For Local
});

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>(options=>
{
	options.Password.RequiredLength = 8;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireDigit = true;

	options.Lockout.MaxFailedAccessAttempts = 5;
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

	options.User.RequireUniqueEmail = true;
	options.SignIn.RequireConfirmedEmail = true;	
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Account/Login"; // Redirect here if not autheticated
	options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect here policy-claim not matched. Ex- 'Department >> HR'
	options.SlidingExpiration = true;
});

// add api for email notification
builder.Services.AddHttpClient("EmailNotificationAPI", client =>
{

	client.BaseAddress = new Uri(builder.Configuration["EmailNotificationEndPoint"]); // For Local
	if (builder.Configuration["Feature"] == "global")
		client.BaseAddress = new Uri(configEnvironmentVariables["EmailNotificationEndPoint"] ?? throw new InvalidOperationException("Email Notification Endpoint not found."));
}).ConfigurePrimaryHttpMessageHandler(() =>
new SocketsHttpHandler
{
	PooledConnectionLifetime = TimeSpan.FromMinutes(2)
});
builder.Services.AddTransient<IEmailService, EmailService>(); 
 
var app = builder.Build();

#region -- minimal api for testing key vault integration --
//app.MapGet("/config-value", (IConfiguration config) =>
//{
//	// A secret named "Database--ConnectionString" in Key Vault 
//	// maps directly to config["Database:ConnectionString"]
//	var secretValue1 = config["KeyVaultSecretName"];
//	var secretValue2 = config["KeyVaultName"];
//	var secretValue3 = config["KeyVaultURL"];
//	return Results.Ok($"Value from configuration: KeyVaultSecretName = {secretValue1} || , KeyVaultName = {secretValue2} ||  , KeyVaultURL = {secretValue3}");
//});

//app.MapGet("/config-value1", (IConfiguration config) =>
//{
//	return Results.Ok($"Value from configuration: connectionString = {connectionStringSecret}");
//});

//var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
//app.MapGet("/get-secret", async () =>
//{
//		return Results.Ok(new { SecretValue = secret.Value });
//});
#endregion

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthorization();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
	 .WithStaticAssets();

app.Run();
