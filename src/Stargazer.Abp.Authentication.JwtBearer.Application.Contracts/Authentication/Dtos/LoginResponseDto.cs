namespace Stargazer.Abp.Authentication.JwtBearer.Application.Contracts.Authentication.Dtos
{
    public class LoginResponseDto
    {
        public string RefreshToken { get; set; } = "";

        public string AccessToken { get; set; } = "";

        public DateTime ExpiresTime { get; set; }
    }
}