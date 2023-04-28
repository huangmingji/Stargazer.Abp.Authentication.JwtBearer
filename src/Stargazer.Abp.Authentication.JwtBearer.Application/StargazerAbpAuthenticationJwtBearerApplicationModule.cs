using Stargazer.Abp.Authentication.JwtBearer.Application.Contracts.AccessToken;
using Stargazer.Abp.Authentication.JwtBearer.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace Stargazer.Abp.Authentication.JwtBearer.Application;

[DependsOn(
    typeof(AbpDddApplicationModule)
)]
public class StargazerAbpAuthenticationJwtBearerApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IAccessTokenGenerator, AccessTokenGenerator>();
    }
}