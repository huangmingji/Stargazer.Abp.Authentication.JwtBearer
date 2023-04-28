using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Stargazer.Abp.Authentication.JwtBearer.Application.Authentication;

public static class AuthenticationExtensions
{
    public static void UseJwtBearerAuthentication(this IServiceCollection services, string[] permissions)
    {
        IConfiguration? configuration = services.BuildServiceProvider().GetService<IConfiguration>();
        var securityKey = configuration?.GetSection("JwtBearer:SecurityKey").Value ?? "123456";
        var issuer = configuration?.GetSection("JwtBearer:Issuer").Value ?? "123456";
        var audience = configuration?.GetSection("JwtBearer:Audience").Value ?? "123456";
            
        services.AddAuthorization(options =>
        {
            foreach (var permission in permissions)
            {
                options.AddPolicy(permission, policy => policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        }).AddAuthentication(options =>
        {                    
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
            jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(securityKey)),//秘钥
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    RequireExpirationTime = true
                };
            }
        );
        services.AddTransient<IAuthorizationHandler, PermissionHandler>();
    }
}