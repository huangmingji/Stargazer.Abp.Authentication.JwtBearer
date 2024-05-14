using Newtonsoft.Json;

namespace Stargazer.Abp.Authentication.JwtBearer.Application.Contracts.Authentication.Dtos
{
    public class LoginResponseDto
    {
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = "";

        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = "";

        [JsonProperty("expires_time")]
        public long ExpiresTime { get; set; }

    }
}