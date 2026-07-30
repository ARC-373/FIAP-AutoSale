using AutoSale.Api.Authentication;
using AutoSale.Api.Authorization;
using AutoSale.Api.Extensions;
using AutoSale.Api.Middleware;
using AutoSale.Infrastructure;
using AutoSale.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AutoSaleDbContext>("postgresql");
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(
        serviceName: builder.Configuration["OTEL_SERVICE_NAME"] ?? "autosale-api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AutoSaleDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();
if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapOpenApi();

app.MapScalarApiReference("/docs", options => options
    .AddPreferredSecuritySchemes("CognitoOAuth")
    .AddAuthorizationCodeFlow("CognitoOAuth", flow =>
    {
        flow.ClientId = builder.Configuration["Authentication:ClientId"];
        flow.Pkce = Pkce.Sha256;
        flow.SelectedScopes = ["openid", "profile", "email"];
    }));

app.Run();

public partial class Program;
