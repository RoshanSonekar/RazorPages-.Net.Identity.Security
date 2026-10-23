using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args); 
var assembly = typeof(Program).Assembly;
string connectionString = builder.Configuration.GetConnectionString("MyPostgresDB") ?? string.Empty; 
if (string.IsNullOrWhiteSpace(connectionString))
{
	throw new ArgumentException(
			"Database connection string 'SQLServer' is missing or empty in configuration.",
			nameof(connectionString)
	);
}

// Add native OpenAPI document generation
builder.Services.AddOpenApi(); //

// Add services to the container.
builder.Services.AddMediatR(config =>
{
	config.RegisterServicesFromAssembly(assembly);
	// Add validation and logging behavior to the MediatR pipeline
	config.AddOpenBehavior(typeof(ValidationBehavior<,>));
	config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Add FluentValidation services
builder.Services.AddValidatorsFromAssembly(assembly);

// Add Carter for minimal API
builder.Services.AddCarter();

builder.Services.AddMarten(options =>
{
	options.Connection(connectionString!); // Roshan#2026@ DB password
}).UseLightweightSessions();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddHealthChecks()
	.AddNpgSql(connectionString!);

var app = builder.Build();

app.MapCarter();
app.UseExceptionHandler(options => { });
app.UseHealthChecks("/health",
	new HealthCheckOptions
	{
		ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
	});

app.MapOpenApi();
app.MapScalarApiReference(); // Exposes /scalar/v1

app.Run();
 