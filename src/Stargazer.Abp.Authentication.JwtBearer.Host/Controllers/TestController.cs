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
using Lemon.Common.Extend;

namespace Stargazer.Abp.Authentication.JwtBearer.Host.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : AbpController
{

    private readonly IConfiguration _configuration;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly IStringEncryptionService _stringEncryptionService;
    private readonly ILogger<TestController> _logger;

    public TestController(IConfiguration configuration,
        IAccessTokenGenerator accessTokenGenerator,
        IStringEncryptionService stringEncryptionService,
        ILogger<TestController> logger)
    {
        _configuration = configuration;
        _accessTokenGenerator = accessTokenGenerator;
        _stringEncryptionService = stringEncryptionService;
        _logger = logger;
    }

    private double RefreshTime { get { return _configuration.GetSection("JwtBearer:RefreshTime").Value.ToDoubleOrNull() ?? 1800; } }
    private double ExpiresTime { get { return _configuration.GetSection("JwtBearer:ExpiresTime").Value.ToDoubleOrNull() ?? 300; } }

    [HttpPost("login")]
    public async Task<LoginResponseDto> LoginAsync([FromBody] VerifyPasswordDto input)
    {
        string userId = Guid.NewGuid().ToString();
        var expiresTime = DateTime.Now.AddSeconds(ExpiresTime);
        var refreshToken = _accessTokenGenerator.GenerateToken(
            userId,
            expiresTime
        );
        var refreshTime = DateTime.Now.AddSeconds(RefreshTime);
        var accessToken = _accessTokenGenerator.GenerateToken(
            userId,
            null,
            new List<string>(),
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

        _logger.LogInformation(refreshToken);

        var tokenValidationResult = _accessTokenGenerator.ValidateToken(refreshToken);
        var userIdStr = tokenValidationResult.Claims.GetOrDefault(AbpClaimTypes.UserId).ToString();
        if (!Guid.TryParse(userIdStr, out Guid userId))
        {
            throw new AbpAuthorizationException();
        }

        var expiresTime = DateTime.Now.AddSeconds(ExpiresTime);
        refreshToken = _accessTokenGenerator.GenerateToken(
            userIdStr,
            expiresTime
        );

        var refreshTime = DateTime.Now.AddSeconds(RefreshTime);
        var accessToken = _accessTokenGenerator.GenerateToken(
            userIdStr,
            null,
            new List<string>(),
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
