using Microsoft.AspNetCore.Authorization;

namespace Stargazer.Abp.Authentication.JwtBearer.Application.Authentication;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
        
}