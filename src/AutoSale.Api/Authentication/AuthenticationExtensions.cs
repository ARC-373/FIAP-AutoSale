using AutoSale.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AutoSale.Api.Authentication;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAutoSaleAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Authentication:Authority"];
                options.Audience = configuration["Authentication:Audience"];
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
            });

        return services;
    }
}
