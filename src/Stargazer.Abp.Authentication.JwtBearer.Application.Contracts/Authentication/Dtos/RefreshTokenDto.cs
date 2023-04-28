namespace Stargazer.Abp.Authentication.JwtBearer.Application.Contracts.Authentication.Dtos
{
    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = "";

        public string Token { get; set; } = "";
    }
}