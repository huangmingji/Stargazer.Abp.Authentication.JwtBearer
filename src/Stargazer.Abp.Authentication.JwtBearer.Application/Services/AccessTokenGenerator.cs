using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Stargazer.Abp.Authentication.JwtBearer.Application.Contracts.AccessToken;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;

namespace Stargazer.Abp.Authentication.JwtBearer.Application.Services;

public class AccessTokenGenerator : ITransientDependency, IAccessTokenGenerator
{
    private readonly IConfiguration _configuration;

    public AccessTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string securityKey { get { return _configuration?.GetSection("JwtBearer:SecurityKey").Value ?? "123456"; } }
    private string issuer { get { return _configuration?.GetSection("JwtBearer:Issuer").Value ?? "123456"; } }
    private string audience { get { return _configuration?.GetSection("JwtBearer:Audience").Value ?? "123456"; } }

    public TokenValidationResult ValidateToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = issuer,
            ValidAudience = audience,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(securityKey))
        };

        var tokenValidationResult = new JsonWebTokenHandler().ValidateToken(token, tokenValidationParameters);
        if (!tokenValidationResult.IsValid)
        {
            // Handle each exception which tokenValidationResult can contain as appropriate for your service
            // Your service might need to respond with a http response instead of an exception.
            if (tokenValidationResult.Exception != null)
                throw tokenValidationResult.Exception;

            throw new AbpAuthorizationException();
        }
        return tokenValidationResult;
    }

    public string GenerateToken(string userId, Guid? tenantId, List<string> permissions, DateTime expires)
    {
        List<Claim> claims = new List<Claim>();
        claims.Add(new Claim(AbpClaimTypes.UserId, userId));

        if (tenantId != null)
        {
            claims.Add(new Claim(AbpClaimTypes.TenantId, tenantId.ToString()));
        }

        foreach (string permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        return GenerateToken(securityKey, audience, issuer, claims, expires);
    }

    public string GenerateToken(List<Claim> claims, DateTime expires)
    {
        return GenerateToken(securityKey, audience, issuer, claims, expires);
    }

    public string GenerateToken(string securityKey, string issuer, string audience, List<Claim> claims,
        DateTime expires)
    {
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(securityKey));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.Now,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateToken(string userId, DateTime expires)
    {
        List<Claim> claims = new List<Claim>();
        claims.Add(new Claim(AbpClaimTypes.UserId, userId));
        return GenerateToken(securityKey, audience, issuer, claims, expires);
    }
}