using AutoSale.Api.Authentication;
using AutoSale.Api.Authorization;
using AutoSale.Api.Extensions;
using AutoSale.Api.Middleware;
using AutoSale.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("AutoSale");
if (string.IsNullOrWhiteSpace(connectionString))
{
    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException("ConnectionStrings:AutoSale must be configured outside the Development environment.");
    }

    connectionString = "Host=localhost;Database=autosale";
}

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<ExceptionHandlingMiddleware>();
builder.Services.AddAutoSaleAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddAutoSaleAuthorization();
builder.Services.AddApplicationHandlers();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapOpenApi();
app.MapScalarApiReference("/docs");

app.Run();

public partial class Program;
