using Microsoft.AspNetCore.Authorization;
using WebAppRazor_Security.Authorisation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddAuthentication().AddCookie("MyCookieAuth", options =>
{
	options.Cookie.Name = "MyCookieAuth";
	options.ExpireTimeSpan= TimeSpan.FromMinutes(5);
	options.LoginPath = "/Account/Login"; // Redirect here if not autheticated
	options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect here policy-claim not matched. Ex- 'Department >> HR'
}
);

builder.Services.AddAuthorization(options=>
{
	options.AddPolicy("AdminOnly", policy => policy.RequireClaim("Admin"));

	options.AddPolicy("MustBelongsToHR", policy => policy.RequireClaim("Department", "HR"));

	options.AddPolicy("HRManagerOnly", policy => policy
	.RequireClaim("Department", "HR")
	.RequireClaim("Manager")
	.Requirements.Add(new HRManagerProbationRequirement(3))); // custom authorisation policy // add value in config
});

builder.Services.AddSingleton<IAuthorizationHandler, HRManagerProbationRequirementHandler>();

// add web api
builder.Services.AddHttpClient("WebAPI", client =>
{
	client.BaseAddress = new Uri("https://localhost:7291");
});

// add session
builder.Services.AddSession(options =>
{
	options.Cookie.HttpOnly= true;
	options.IdleTimeout =TimeSpan.FromMinutes(10);
	options.Cookie.IsEssential=true;
});

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

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapStaticAssets();
app.MapRazorPages()
	 .WithStaticAssets();

app.Run();
