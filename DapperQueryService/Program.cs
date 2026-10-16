using BuildingBlocks.Behaviors;
using DapperQueryService.Infra;
using DapperQueryService.MongoDbContext;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Scalar.AspNetCore;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// SQL Connection for Dapper
string connectionString = builder.Configuration.GetConnectionString("SQLServer");
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

var app = builder.Build();


app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
