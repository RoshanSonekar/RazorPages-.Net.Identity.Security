using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Account;
using WebApp.Integration.Email;
using WebAppAuthentication.Integration.DapperService;
using WebAppAuthentication.Integration.UserProfileAPI;
using Microsoft.AspNetCore.Mvc.RazorPages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

string connectionString =  builder.Configuration.GetConnectionString("SqlServer") ?? throw new InvalidOperationException("Connection string 'SqlServer' not found.");
IConfiguration configEnvironmentVariables = builder.Configuration;
string AppId = string.Empty; string AppSecret = string.Empty; string ClientId = string.Empty; string ClientSecret = string.Empty;
if (builder.Configuration["Feature"] == "Global")
{
	#region --- Fetch Azure SQL DB Connectionstring. For Azure Deployment use 'Environment Variables' to get configuration for AzureVault 'url' and 'secret name'---
	string keyVaultUri = configEnvironmentVariables["KeyVaultURL"] ?? throw new InvalidOperationException("Key Vault URI not found.");
	var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
	KeyVaultSecret secret = await client.GetSecretAsync(configEnvironmentVariables["KeyVaultSecretName"] ?? throw new InvalidOperationException("Key Vault Secret Name not found."));
	connectionString = secret.Value ?? throw new InvalidOperationException("Secret value not found.");

	secret = await client.GetSecretAsync("FacebookAppId") ?? throw new InvalidOperationException("Key Vault Secret Name not found.");
	AppId = secret.Value ?? throw new InvalidOperationException("Secret value not found.");

	secret = await client.GetSecretAsync("FacebookAppSecret") ?? throw new InvalidOperationException("Key Vault Secret Name not found.");
	AppSecret = secret.Value ?? throw new InvalidOperationException("Secret value not found.");

	secret = await client.GetSecretAsync("GoogleClientId") ?? throw new InvalidOperationException("Key Vault Secret Name not found.");
	ClientId = secret.Value ?? throw new InvalidOperationException("Secret value not found.");

	secret = await client.GetSecretAsync("GoogleClientSecret") ?? throw new InvalidOperationException("Key Vault Secret Name not found.");
  ClientSecret = secret.Value ?? throw new InvalidOperationException("Secret value not found.");
	#endregion
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{ 
	options.UseSqlServer(connectionString);
	//options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")); // For Local
});

// Add Identity
builder.Services.AddIdentity<IdentityUserExtension, IdentityRole>(options=>
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

#region ------------- Integration / External Services -------------
#region ------------- email notification service ------------- 
builder.Services.AddHttpClient("EmailNotificationAPI", client =>
{
	client.BaseAddress = new Uri(builder.Configuration["EmailNotificationEndPoint"] ?? throw new InvalidOperationException("Email Notification Endpoint not found.")); // For Local
	if (builder.Configuration["Feature"] == "Global")
		client.BaseAddress = new Uri(configEnvironmentVariables["EmailNotificationEndPoint"] ?? throw new InvalidOperationException("Email Notification Endpoint not found."));
}).ConfigurePrimaryHttpMessageHandler(() =>
new SocketsHttpHandler
{
	PooledConnectionLifetime = TimeSpan.FromMinutes(2)
});
builder.Services.AddTransient<IEmailService, EmailService>();
#endregion

#region ------------- User profile service ------------- 
builder.Services.AddHttpClient("UserProfileService", client =>
{
	client.BaseAddress = new Uri(builder.Configuration["UserProfileService"] ?? throw new InvalidOperationException("UserProfileService not found.")); // For Local
	if (builder.Configuration["Feature"] == "Global")
		client.BaseAddress = new Uri(configEnvironmentVariables["UserProfileService"] ?? throw new InvalidOperationException("UserProfileService not found."));
}).ConfigurePrimaryHttpMessageHandler(() =>
new SocketsHttpHandler
{
	PooledConnectionLifetime = TimeSpan.FromMinutes(2)
});
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
#endregion

#region ------------- User transaction service ------------- 
builder.Services.AddHttpClient("UserTransactionDapperService", client =>
{
	client.BaseAddress = new Uri(builder.Configuration["UserTransactionDapperService"] ?? throw new InvalidOperationException("UserTransactionDapperService not found.")); // For Local
	if (builder.Configuration["Feature"] == "Global")
		client.BaseAddress = new Uri(configEnvironmentVariables["UserTransactionDapperService"] ?? throw new InvalidOperationException("UserTransactionDapperService not found."));
}).ConfigurePrimaryHttpMessageHandler(() =>
new SocketsHttpHandler
{
	PooledConnectionLifetime = TimeSpan.FromMinutes(2)
});
builder.Services.Scan(scan => scan
.FromAssemblyOf<DapperSqlService>()
.AddClasses()
.AsMatchingInterface()
.WithTransientLifetime()); //builder.Services.AddTransient<IDapperSqlService, DapperSqlService>();
#endregion
#endregion

#region --- OAuth2.0 Authentication for Facebook and Google ---
builder.Services.AddAuthentication().AddFacebook(options =>
{
	if (builder.Configuration["Feature"] == "Global")
	{
		options.AppId = AppId;
		options.AppSecret = AppSecret;
	}
	else
	{
		options.AppId = builder.Configuration["Facebook:AppId"] ?? throw new InvalidOperationException("Facebook AppId not found.");
		options.AppSecret = builder.Configuration["Facebook:AppSecret"] ?? throw new InvalidOperationException("Facebook AppSecret not found.");
	}
});

builder.Services.AddAuthentication().AddGoogle(options =>
{
	if (builder.Configuration["Feature"] == "Global")
	{
		options.ClientId = ClientId;
		options.ClientSecret = ClientSecret;
	}
	else
	{
		options.ClientId = builder.Configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId not found.");
		options.ClientSecret = builder.Configuration["Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not found.");
	}
});
#endregion

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
app.UseAuthentication();
app.UseAuthorization(); 

app.MapStaticAssets();
app.MapRazorPages()
	 .WithStaticAssets();

app.MapControllers();
app.Run();
