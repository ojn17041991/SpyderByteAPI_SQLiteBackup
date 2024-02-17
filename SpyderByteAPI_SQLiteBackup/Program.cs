using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        services.AddScoped<IHttpService, HttpService>();
        services.AddHttpClient();
    })
    .Build();

host.Run();
