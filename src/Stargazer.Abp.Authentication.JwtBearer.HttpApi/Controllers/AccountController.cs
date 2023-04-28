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

    [HttpPost("login")]
    public async Task<LoginResponseDto> LoginAsync([FromBody] VerifyPasswordDto input)
    {
        var user = await _userService.VerifyPasswordAsync(input);
        var refreshToken = _accessTokenGenerator.GenerateToken(
            user.Id.ToString(),
            DateTime.Now.AddHours(8)
        );
        var expiresTime = DateTime.Now.AddMinutes(5);
        var token = _accessTokenGenerator.GenerateToken(
            user.Id.ToString(),
            user.UserRoles.FirstOrDefault()?.TenantId,
            user.GetPermissions(),
            expiresTime
        );

        return new LoginResponseDto()
        {
            RefreshToken = _stringEncryptionService.Encrypt(refreshToken),
            AccessToken = token,
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
        var expiresTime = DateTime.Now.AddMinutes(5);
        var token = _accessTokenGenerator.GenerateToken(
            user.Id.ToString(),
            user.UserRoles.FirstOrDefault()?.TenantId,
            user.GetPermissions(),
            expiresTime
        );

        return new LoginResponseDto()
        {
            RefreshToken = input.RefreshToken,
            AccessToken = token,
            ExpiresTime = expiresTime
        };
    }
}