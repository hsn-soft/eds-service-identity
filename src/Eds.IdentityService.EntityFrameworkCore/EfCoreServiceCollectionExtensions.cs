using Eds.IdentityService.Domain.AppRoleDomain.Repositories;
using Eds.IdentityService.Domain.AppUserDomain.Repositories;
using Eds.IdentityService.EntityFrameworkCore.Context;
using Eds.IdentityService.EntityFrameworkCore.Repositories;
using HsnSoft.Base.Auditing;
using HsnSoft.Base.Data;
using HsnSoft.Base.Domain.Repositories;
using HsnSoft.Base.Domain.Services;
using HsnSoft.Base.EntityFrameworkCore;
using HsnSoft.Base.Timing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eds.IdentityService.EntityFrameworkCore;

public static class EfCoreServiceCollectionExtensions
{
    public static IServiceCollection AddServiceEfCoreDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBaseTimingServiceCollection();
        services.Configure<BaseClockOptions>(o => o.Kind = DateTimeKind.Utc);
        services.AddTransient<IAuditPropertySetter, AuditPropertySetter>();
        services.AddBaseDataServiceCollection();

        AddAuthServerJwtDatabaseConfiguration(services, configuration);

        // Must be Scoped => Cannot consume any scoped service and CurrentUser object creation on constructor
        services.AddTransient<IAppUserRepository, AppUserRepository>();
        services.AddTransient<IAppRoleRepository, AppRoleRepository>();

        services.AddDbContext<IdentityServiceDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString(EfCoreDbProperties.ConnectionStringName), sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory");
                    sqlOptions.MigrationsAssembly(typeof(IdentityServiceDbContext).Assembly.GetName().Name);
                    sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(6), errorCodesToAdd: null);
                    sqlOptions.CommandTimeout(30000);
                    sqlOptions.MaxBatchSize(100);
                });
                // options.EnableSensitiveDataLogging();
                // options.UseLoggerFactory(LoggerFactory.Create(builder =>
                // {
                //     builder.AddConsole();
                //     builder.SetMinimumLevel(LogLevel.Information);
                // }));
                options.EnableSensitiveDataLogging(false);
            }
            , contextLifetime: ServiceLifetime.Scoped // Must be Scoped => Cannot consume any scoped service and CurrentUser object creation on constructor
            , optionsLifetime: ServiceLifetime.Singleton
        );

        // unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork<IdentityServiceDbContext>>();

        // Must be Scoped => Cannot consume any scoped service and CurrentUser object creation on constructor
        services.AddScoped(typeof(IEfCoreGenericRepository<,>), typeof(EfCoreGenericRepository<,>));
        // services.AddScoped<IFakeRepository, EfCoreFakeRepository>();

        return services;
    }

    public static IServiceCollection AddAuthServerJwtDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityAppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString(EfCoreDbProperties.ConnectionStringName), sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory");
                sqlOptions.MigrationsAssembly(typeof(IdentityAppDbContext).Assembly.GetName().Name);
                sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(6), errorCodesToAdd: null);
                sqlOptions.CommandTimeout(30000);
                sqlOptions.MaxBatchSize(100);
            });
            // options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
            options.EnableSensitiveDataLogging(false);
        });

        return services;
    }
}