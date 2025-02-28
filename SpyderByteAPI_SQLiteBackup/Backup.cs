using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SpyderByteAPI_SQLiteBackup.Services.Abstract;

namespace SpyderByteAPI_SQLiteBackup
{
    public class Backup
    {
        private readonly ILogger _logger;
        private readonly IDataService _dataService;

        public Backup(ILoggerFactory loggerFactory, IDataService dataService)
        {
            _logger = loggerFactory.CreateLogger<Backup>();
            _dataService = dataService;
        }

        [Function("Backup")]
        public async Task RunBackup([TimerTrigger("0 0 0 1 * *", RunOnStartup = true)] TimerInfo timer)
        {
            return;

            _logger.LogInformation($"Database Backup requested at {DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss.fffZ")}.");

            if (await _dataService.RequestBackup())
            {
                _logger.LogInformation($"Database Backup Function App completed successful.");
            }
            else
            {
                _logger.LogInformation($"Database Backup Function App failed to complete.");
            }

            if (timer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next database backup scheduled for {timer.ScheduleStatus.Next.ToString("yyyy-MM-ddThh:mm:ss.fffZ")}");
            }
        }
    }
}
