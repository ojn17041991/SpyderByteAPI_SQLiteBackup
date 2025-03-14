using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpyderByteAPI_SQLiteBackup.Services;
using SpyderByteAPI_SQLiteBackup.Services.Abstract;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(con =>
    {
        con.AddUserSecrets<Program>(optional: true, reloadOnChange: false);
    })
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.Configure<LoggerFilterOptions>(options =>
        {
            var filterRule = options.Rules.FirstOrDefault(rule => rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider")!;
            if (filterRule is not null)
            {
                options.Rules.Remove(filterRule);
            }
        });
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IDataService, DataService>();
        services.AddHttpClient();
    })
    .Build();

host.Run();
