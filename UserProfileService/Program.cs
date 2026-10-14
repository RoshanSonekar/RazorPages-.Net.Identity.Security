using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UserProfileService.Data;
using UserProfileService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// Add services to the container.

builder.Services.AddControllers();

// Add db context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
		options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));

// Application Services
builder.Services.AddScoped<IUserProfile, UserProfile>();

var app = builder.Build();

app.MapOpenApi(); 
app.MapScalarApiReference();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
