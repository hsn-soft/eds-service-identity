using Autofac;
using Autofac.Extensions.DependencyInjection;
using Eds.IdentityService.Application;
using Hhs.IdentityService.Domain.AppRoleDomain.Entities;
using Hhs.IdentityService.Domain.AppUserDomain.Entities;
using Hhs.IdentityService.Domain.Localization;
using Eds.IdentityService.EntityFrameworkCore;
using Eds.IdentityService.EntityFrameworkCore.Context;
using Hhs.Shared.Contracts.Cache.ServicePermissions;
using Hhs.Shared.Contracts.Events;
using Hhs.Shared.Hosting;
using Hhs.Shared.Hosting.Microservices;
using Hhs.Shared.Hosting.Microservices.Middlewares;
using Hhs.Shared.Hosting.Middlewares;
using HsnSoft.Base.AspNetCore.Localization;
using Microsoft.AspNetCore.Identity;

namespace Hhs.IdentityService;

public sealed class Startup
{
    private IConfiguration Configuration { get; }
    private IWebHostEnvironment WebHostEnvironment { get; }

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        WebHostEnvironment = environment;
    }

    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.ConfigureMicroserviceHost(Configuration)
            .AddAdvancedController(Configuration, typeof(Startup))
            .AddJwtServerAuthentication(Configuration, WebHostEnvironment, "audience-service-identity")
            .AddCustomAuthorization(IdentityServicePermissions.GetAll())
            .AddEventBus(Configuration, typeof(EventHandlersAssemblyMarker).Assembly)
            .AddMicroserviceUserTenantChecker()
            .AddHostingHealthChecks(Configuration, "identity", checkRedis: true, checkBroker: true,
                checkPostgresql: true, postgresqlConnectionName: EfCoreDbProperties.ConnectionStringName)
            .AddServiceApplicationConfiguration(Configuration)
            .AddServiceDatabaseConfiguration(Configuration);

        services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.AllowedUserNameCharacters = "abcçdefghiıjklmnoöpqrsştuüvwxyzABCÇDEFGHIİJKLMNOÖPQRSŞTUÜVWXYZ0123456789-._@+'#!/^%{}*";
            })
            .AddEntityFrameworkStores<IdentityAppDbContext>()
            .AddDefaultTokenProviders();

        if (!WebHostEnvironment.IsHostProduction())
        {
            SwaggerConfigurationHelper.ConfigureWithBearer(services,
                "Please enter a valid token. Token audiences contains audience-service-identity",
                $"{Program.AppName} API");
        }

        var container = new ContainerBuilder();
        container.Populate(services);

        return new AutofacServiceProvider(container.Build());
    }

    public void Configure(IApplicationBuilder app, IHostApplicationLifetime hostApplicationLifetime)
    {
        if (!WebHostEnvironment.IsHostProduction())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{Program.AppName} API");
            });
        }

        app.UseLocalization(typeof(IdentityServiceResource));

        /*Middleware*/
        app.UseMiddleware<RequestResponseLoggerMiddleware>();
        app.UseMiddleware<BaseLocalizationMiddleware>();
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseUserTenantChecker();

        app.UseEndpoints(endpoints =>
        {
            if (!WebHostEnvironment.IsHostProduction())
            {
                endpoints.MapDefaultControllerRoute();
            }
            else
            {
                endpoints.MapControllers();

                var buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER");
                var appVersion = !string.IsNullOrWhiteSpace(buildNumber) ? $"v1.0.{buildNumber}" : "v1.0.0";
                endpoints.MapGet("/", () => $"HHS {Program.AppName} | {Program.AppId} | {WebHostEnvironment.EnvironmentName} | {appVersion}");
            }
        });
        app.UseHostingHealthChecks();

        // Subscribe all event handlers
        app.UseEventBus(typeof(EventHandlersAssemblyMarker).Assembly, new Dictionary<string, ushort>()
        {
            { nameof(CachePermissionGrantsChangedEto), 1 }
        });

        hostApplicationLifetime.ApplicationStopping.Register(OnShutdown);
    }

    private static void OnShutdown() => Console.WriteLine("Stopping web host ({0})...", Program.AppName);
}