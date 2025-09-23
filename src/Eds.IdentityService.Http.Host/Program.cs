using Eds.IdentityService.Application;
using HsnSoft.Base.AspNetCore.Serilog;
using HsnSoft.Base.Tracing;
using Microsoft.AspNetCore;
using Serilog;

namespace Eds.IdentityService;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        string workspace = typeof(Startup).Namespace;
        ApplicationIdentifier.AppId = Guid.NewGuid().ToString("N");
        ApplicationIdentifier.AppName = workspace?[(workspace.IndexOf('.') + 1)..];

        Log.Logger = SerilogConfigurationHelper.ConfigureConsoleLogger(GetConfiguration());

        try
        {
            Log.Information("Configuring web host ({ApplicationContext})...", ApplicationIdentifier.AppName);
            var host = CreateHostBuilder(args);

            // using (var scope = host.Services.CreateScope())
            // {
            // }

            Log.Information("Starting web host ({ApplicationContext})...", ApplicationIdentifier.AppName);
            await host.RunAsync();

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", ApplicationIdentifier.AppName);
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IWebHost CreateHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .CaptureStartupErrors(false)
            .ConfigureKestrel((context, options) =>
            {
                options.Limits.MaxRequestBufferSize = long.MaxValue;
                options.Limits.MaxRequestBodySize = long.MaxValue;

                var env = context.HostingEnvironment;
                if (!env.IsDevelopment()) return;

                options.ListenAnyIP(6610);
            })
            .ConfigureAppConfiguration(x => x.AddConfiguration(GetConfiguration()))
            .UseStartup<Startup>()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog(Log.Logger);
            })
            .Build();

    private static IConfiguration GetConfiguration() =>
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json")
            .AddEnvironmentVariables()
            .Build();
}