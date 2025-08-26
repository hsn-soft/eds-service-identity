using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace Eds.IdentityService.EntityFrameworkCore.Factory;

internal static class DbContextFactoryHelper
{
    [CanBeNull]
    internal static string GetConnectionStringFromConfiguration()
        => BuildConfiguration().GetConnectionString(EfCoreDbProperties.ConnectionStringName);
        // => "Host=edsahin.com;Port=35432;Database=Eds_IdentityService;User ID=postgres;Password=postgres;Pooling=true;Connection Lifetime=0;";

    private static IConfigurationRoot BuildConfiguration()
    {
        const string serviceName = "IdentityService";

        IConfiguration targetLaunchSetting = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()
                , $"..{Path.DirectorySeparatorChar}Eds.{serviceName}.Http.Host{Path.DirectorySeparatorChar}Properties"))
            .AddJsonFile("launchSettings.json")
            .Build();
        var environmentName = targetLaunchSetting[$"profiles:{serviceName}:environmentVariables:ASPNETCORE_ENVIRONMENT"];
        Console.WriteLine($"ASPNETCORE_ENVIRONMENT:{targetLaunchSetting[$"profiles:{serviceName}:environmentVariables:ASPNETCORE_ENVIRONMENT"]}");

        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()
                , $"..{Path.DirectorySeparatorChar}Eds.{serviceName}.Http.Host"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: false);

        return builder.Build();
    }
}