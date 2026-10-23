using BuildingBlocks.Behaviors;
using DapperQueryService.Infra;
using DapperQueryService.MongoDbContext;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.Data.SqlClient;
using Scalar.AspNetCore;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// SQL Connection for Dapper
string connectionString = builder.Configuration.GetConnectionString("SQLServer") ?? string.Empty;
if (string.IsNullOrWhiteSpace(connectionString))
{
	throw new ArgumentException(
			"Database connection string 'SQLServer' is missing or empty in configuration.",
			nameof(connectionString)
	);
}

builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(connectionString));

// Application Services MediatR
builder.Services.AddMediatR(config =>
{
	config.RegisterServicesFromAssembly(assembly);
	config.AddOpenBehavior(typeof(ValidationBehavior<,>));
	config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Add FluentValidation services
builder.Services.AddValidatorsFromAssembly(assembly);

// Explicitly register the transaction handlers from the Application project (MongoDB)
builder.Services.AddTransient<MediatR.IRequestHandler<DapperQueryService.Application.AddTranactionCommand, DapperQueryService.Application.AddTransactionResult>, DapperQueryService.Application.TransactionCommandHandler>();
builder.Services.AddTransient<MediatR.IRequestHandler<DapperQueryService.Application.GetTransactionsQuery, DapperQueryService.Application.GetTransactionsResult>, DapperQueryService.Application.TransactionsQueryHandler>();
builder.Services.AddTransient<MediatR.IRequestHandler<DapperQueryService.Application.GetTransactionQuery, DapperQueryService.Application.GetTransactionResult>, DapperQueryService.Application.TransactionQueryHandler>();

// Application Services
builder.Services.AddScoped<IContextMongoDb, ContextMongoDb>(); 
builder.Services.AddScoped<IUserTransactions, UserTransactions>();

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
app.UseAuthorization();
app.MapControllers();

app.Run();
