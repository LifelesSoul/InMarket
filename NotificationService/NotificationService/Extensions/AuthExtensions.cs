using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NotificationService.API.Configuration;

namespace NotificationService.API.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAuth0Authentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Auth0Settings>(configuration.GetSection("Auth0"));

        var auth0Settings = configuration.GetSection("Auth0").Get<Auth0Settings>()
            ?? throw new InvalidOperationException("Auth0 settings are missing in Configuration!");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://{auth0Settings.Domain}/";
                options.Audience = auth0Settings.Audience;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    RoleClaimType = "https://inmarket-api/roles"
                };
            });

        return services;
    }
}
