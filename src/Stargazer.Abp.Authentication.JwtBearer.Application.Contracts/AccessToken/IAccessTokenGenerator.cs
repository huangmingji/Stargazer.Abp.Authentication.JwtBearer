using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Stargazer.Abp.Authentication.JwtBearer.Application.Contracts.AccessToken;

public interface IAccessTokenGenerator
{
    TokenValidationResult ValidateToken(string token);

    string GenerateToken(string userId, Guid? tenantId, List<string> permissions, DateTime expires);
    
    string GenerateToken(string userId, DateTime expires);

    string GenerateToken(List<Claim> claims, DateTime expires);
        
    string GenerateToken(string securityKey, string issuer, string audience, List<Claim> claims, DateTime expires);
}