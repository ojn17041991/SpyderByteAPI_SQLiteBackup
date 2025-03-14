using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SpyderByteAPI_SQLiteBackup.Services.Abstract;

namespace SpyderByteAPI_SQLiteBackup
{
    public class Cleanup
    {
        private readonly ILogger _logger;
        private readonly IDataService _dataService;

        public Cleanup(ILoggerFactory loggerFactory, IDataService dataService)
        {
            _logger = loggerFactory.CreateLogger<Cleanup>();
            _dataService = dataService;
        }

        [Function("Cleanup")]
        public async Task RunCleanup([TimerTrigger("0 15 0 1 * *", RunOnStartup = true)] TimerInfo timer)
        {
            _logger.LogInformation($"Database Cleanup requested at {DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss.fffZ")}.");

            if (await _dataService.RequestCleanup())
            {
                _logger.LogInformation($"Database Cleanup Function App completed successful.");
            }
            else
            {
                _logger.LogInformation($"Database Cleanup Function App failed to complete.");
            }

            if (timer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next database cleanup scheduled for {timer.ScheduleStatus.Next.ToString("yyyy-MM-ddThh:mm:ss.fffZ")}");
            }
        }
    }
}
