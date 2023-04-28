using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace Stargazer.Abp.Authentication.JwtBearer.Application.Contracts;

[DependsOn(
    typeof(AbpDddApplicationContractsModule)
)]
public class StargazerAbpAuthenticationJwtBearerApplicationContractsModule : AbpModule
{
        
}