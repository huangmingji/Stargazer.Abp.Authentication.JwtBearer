using Stargazer.Abp.Authentication.JwtBearer.Application.Contracts.Authentication.Dtos;
using Stargazer.Abp.Account.Application.Contracts.Users;
using Stargazer.Abp.Account.Application.Contracts.Users.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stargazer.Abp.Authentication.JwtBearer.Application.Contracts.AccessToken;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Security.Claims;
using Volo.Abp.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Dynamic.Core.Tokenizer;
using Lemon.Common.Extend;

namespace Stargazer.Abp.Authentication.JwtBearer.HttpApi.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/account")]
public class AccountController : AbpController
{
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IStringEncryptionService _stringEncryptionService;
    public AccountController(IConfiguration configuration, IUserService userService,
        IAccessTokenGenerator accessTokenGenerator,
        IStringEncryptionService stringEncryptionService)
    {
        _configuration = configuration;
        _userService = userService;
        _accessTokenGenerator = accessTokenGenerator;
        _stringEncryptionService = stringEncryptionService;
    }

    private double RefreshTime { get { return _configuration.GetSection("JwtBearer:RefreshTime").Value.ToDoubleOrNull() ?? 1800; } }
    private double ExpiresTime { get { return _configuration.GetSection("JwtBearer:ExpiresTime").Value.ToDoubleOrNull() ?? 300; } }

    [HttpPost("login")]
    public async Task<LoginResponseDto> LoginAsync([FromBody] VerifyPasswordDto input)
    {
        var user = await _userService.VerifyPasswordAsync(input);
        var expiresTime = DateTime.Now.AddSeconds(ExpiresTime);
        var refreshToken = _accessTokenGenerator.GenerateToken(
            user.Id.ToString(),
            expiresTime
        );
        var refreshTime = DateTime.Now.AddSeconds(RefreshTime);
        var accessToken = _accessTokenGenerator.GenerateToken(
            user.Id.ToString(),
            user.UserRoles.FirstOrDefault()?.TenantId,
            user.GetPermissions(),
            refreshTime
        );

        return new LoginResponseDto()
        {
            RefreshToken = _stringEncryptionService.Encrypt(refreshToken),
            AccessToken = accessToken,
            RefreshTime = refreshTime,
            ExpiresTime = expiresTime
        };
    }

    [HttpPost("refresh_token")]
    public async Task<LoginResponseDto> RefreshTokenAsync([FromBody] RefreshTokenDto input)
    {
        var refreshToken = _stringEncryptionService.Decrypt(input.RefreshToken);
        var tokenValidationResult = _accessTokenGenerator.ValidateToken(refreshToken);
        var userIdStr = tokenValidationResult.Claims.GetOrDefault(AbpClaimTypes.UserId).ToString();
        if (!Guid.TryParse(userIdStr, out Guid userId))
        {
            throw new AbpAuthorizationException();
        }
        var user = await _userService.GetAsync(userId);

        var expiresTime = DateTime.Now.AddSeconds(ExpiresTime);
        refreshToken = _accessTokenGenerator.GenerateToken(
            user.Id.ToString(),
            expiresTime
        );

        var refreshTime = DateTime.Now.AddSeconds(RefreshTime);
        var accessToken = _accessTokenGenerator.GenerateToken(
            user.Id.ToString(),
            user.UserRoles.FirstOrDefault()?.TenantId,
            user.GetPermissions(),
            refreshTime
        );

        return new LoginResponseDto()
        {
            RefreshToken = _stringEncryptionService.Encrypt(refreshToken),
            AccessToken = accessToken,
            RefreshTime = refreshTime,
            ExpiresTime = expiresTime
        };
    }
}