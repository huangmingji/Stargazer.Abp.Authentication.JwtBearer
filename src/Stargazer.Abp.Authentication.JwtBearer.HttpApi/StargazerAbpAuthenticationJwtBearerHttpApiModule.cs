using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stargazer.Abp.Authentication.JwtBearer.Application.Contracts;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Volo.Abp.Security;
using Volo.Abp.Security.Encryption;

namespace Stargazer.Abp.Authentication.JwtBearer.HttpApi
{
    [DependsOn(
        typeof(StargazerAbpAuthenticationJwtBearerApplicationContractsModule),
        typeof(AbpSecurityModule),
        typeof(AbpAspNetCoreMvcModule)
    )]
    public class StargazerAbpAuthenticationJwtBearerHttpApiModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            IConfiguration? configuration = context.Services.BuildServiceProvider().GetService<IConfiguration>();
            string strongPassPhrase = configuration?.GetSection("App:AbpSecurity:PassPhrase").Value ?? "LxMH1v8lyuVsOF3B";
            string salt = configuration?.GetSection("App:AbpSecurity:Salt").Value ?? "p%TCDNuW";
            string initVectorBytes = configuration?.GetSection("App:AbpSecurity:InitVectorBytes").Value ?? "5uhQorj0IO7eMTBR";
            Configure<AbpStringEncryptionOptions>(opts =>
            {
                opts.DefaultPassPhrase = strongPassPhrase;
                opts.DefaultSalt = Encoding.UTF8.GetBytes(salt);
                opts.InitVectorBytes = Encoding.UTF8.GetBytes(initVectorBytes);
                opts.Keysize = 512;
            });
        }
    }
}