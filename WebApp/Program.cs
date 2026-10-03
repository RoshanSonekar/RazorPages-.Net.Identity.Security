using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Account;
using WebApp.Integration.Email;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer"));
});

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>(options=>
{
	options.Password.RequiredLength = 8;
	options.Password.RequireUppercase = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireDigit = true;

	options.Lockout.MaxFailedAccessAttempts = 5;
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

	options.User.RequireUniqueEmail = true;
	options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Account/Login"; // Redirect here if not autheticated
	options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect here policy-claim not matched. Ex- 'Department >> HR'

});

// add web api for email notification
builder.Services.AddHttpClient("EmailNotificationAPI", client =>
{
	//client.BaseAddress = new Uri("your endpoint");
	client.BaseAddress = new Uri("https://emailnotificationapi-h7gbfbchcxhkauax.southafricanorth-01.azurewebsites.net/api/NotificationService/Email/");
});
builder.Services.AddTransient<IEmailService, EmailService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthorization();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
	 .WithStaticAssets();

app.Run();
