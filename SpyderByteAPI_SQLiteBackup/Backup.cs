using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SpyderByteAPI_SQLiteBackup.Services.Abstract;

namespace SpyderByteAPI_SQLiteBackup
{
    public class Backup
    {
        private readonly ILogger _logger;
        private readonly IHttpService _httpService;

        public Backup(ILoggerFactory loggerFactory, IHttpService httpService)
        {
            _logger = loggerFactory.CreateLogger<Backup>();
            _httpService = httpService;
        }

        [Function("Backup")]
        public async Task Run([TimerTrigger("0 0 0 1 * *")] TimerInfo timer)
        {
            _logger.LogInformation($"Database Backup requested at {DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss.fffZ")}.");

            if (await _httpService.RequestBackup())
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
