using System;
using System.Linq;
using System.Reflection;
using Lemon.Common.Extend;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Stargazer.Abp.Authentication.JwtBearer.Application;
using Stargazer.Abp.Authentication.JwtBearer.HttpApi;

namespace Stargazer.Abp.Authentication.JwtBearer.Host
{
    [DependsOn(
    typeof(StargazerAbpAuthenticationJwtBearerApplicationModule),
    typeof(StargazerAbpAuthenticationJwtBearerHttpApiModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule)
    )]
    public class HostModule : AbpModule
    {
        private const string DefaultCorsPolicyName = "Default";
        private static void ConfigureSwaggerServices(ServiceConfigurationContext context)
        {
            context.Services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Lemon.Abp.Template Service API", Version = "v1" });
                    options.DocInclusionPredicate((docName, description) => true);
                });
        }

        private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
        {

        }

        private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
        {
            context.Services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, builder =>
                {
                    builder
                        .WithOrigins(
                            configuration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .WithAbpExposedHeaders()
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
        }
        
        private void ConfigureDataProtection(ServiceConfigurationContext context, IConfiguration configuration)
        {
            // 添加数据保护服务，设置统一应用程序名称，
            var dataProtectionBuilder = context.Services.AddDataProtection()
                .SetApplicationName(Assembly.GetExecutingAssembly().FullName ?? "Stargazer.Abp.Template");
            if (configuration["Redis:IsEnabled"].ToBool())
            {
                var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);//建立Redis 连接
                // 指定使用Reids存储私钥
                dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
            }
            else
            {
                dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory));
            }
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            var hostingEnvironment = context.Services.GetHostingEnvironment();

            context.Services.AddMvcCore().AddNewtonsoftJson(
                op =>
                {
                    op.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                    op.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                    op.SerializerSettings.Converters.Add(new Ext.DateTimeJsonConverter());
                    op.SerializerSettings.Converters.Add(new Ext.LongJsonConverter());
                });

            // Configure<AbpAspNetCoreMvcOptions>(options =>
            // {
            //     options.ConventionalControllers.Create(typeof(ApplicationModule).Assembly);
            // });

            ConfigureDataProtection(context, configuration);
            ConfigureAuthentication(context, configuration);
            ConfigureSwaggerServices(context);
            ConfigureCors(context, configuration);
        }

        public override void OnApplicationInitialization(
            ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseCors(DefaultCorsPolicyName);

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseConfiguredEndpoints();

            if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Lemon.Abp.Template Service API");
                });

            }

        }
    }
}

