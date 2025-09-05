using Eds.IdentityService.Application.Contracts.AppRoleDomain.Services;
using Eds.IdentityService.Application.Contracts.AppUserDomain.Services;
using Eds.IdentityService.Application.Services;
using Eds.Shared.Contracts.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eds.IdentityService.Application;

public static class AppService
{
    public static string AppId { get; set; }
    public static string AppName { get; set; }
}

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddServiceApplicationConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(ApplicationAutoMapperProfile));

        services.AddSingleton<IServicePermissionProvider, ApplicationPermissionProvider>();

        // Must be Scoped or Transient => Cannot consume any scoped service
        services.AddScoped<IAppUserAppService, AppUserAppService>();
        services.AddScoped<IAppRoleAppService, AppRoleAppService>();

        return services;
    }
}