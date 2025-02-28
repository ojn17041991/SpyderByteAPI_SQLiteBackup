using SpyderByteAPI_SQLiteBackup.Services.Abstract;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace SpyderByteAPI_SQLiteBackup.Services
{
    public class DataService : IDataService
    {
        private ILogger<DataService> _logger;   
        private IHttpClientFactory _httpClientFactory;
        private IAuthenticationService _authenticationService;

        private string _url;
        private string _databaseBackupEndpoint;
        private string _databaseCleanupEndpoint;

        public DataService(IHttpClientFactory httpClientFactory, ILogger<DataService> logger, IAuthenticationService authenticationService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _authenticationService = authenticationService;

            _url = Environment.GetEnvironmentVariable("Url") ?? string.Empty;
            _databaseBackupEndpoint = Environment.GetEnvironmentVariable("Backup:Endpoint") ?? string.Empty;
            _databaseCleanupEndpoint = Environment.GetEnvironmentVariable("Cleanup:Endpoint") ?? string.Empty;
        }

        public async Task<bool> RequestBackup()
        {
            _logger.LogInformation($"Database backup requested.");

            string? token = await _authenticationService.Authenticate();
            if (token == null)
            {
                _logger.LogError("Authentication request failed.");
                return false;
            }

            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    // Add the token to the header.
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Make request for DB backup.
                    var databaseBackupResponse = await httpClient.PostAsync(_url + _databaseBackupEndpoint, null);
                    if (databaseBackupResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Database backup request successful.");
                    }
                    else
                    {
                        _logger.LogError("Database backup request failed.");
                    }
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Failed to connect to API.");
            }

            bool deauthenticationSuccessful = await _authenticationService.Deauthenticate(token);
            if (deauthenticationSuccessful == false)
            {
                _logger.LogError("Deauthentication request failed.");
                return false;
            }

            return true;
        }

        public async Task<bool> RequestCleanup()
        {
            _logger.LogInformation($"Database cleanup requested.");

            string? token = await _authenticationService.Authenticate();
            if (token == null)
            {
                _logger.LogError("Authentication request failed.");
                return false;
            }

            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    // Add the token to the header.
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Make request for DB cleanup.
                    var databaseCleanupResponse = await httpClient.DeleteAsync(_url + _databaseCleanupEndpoint);
                    if (databaseCleanupResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Database cleanup request successful.");
                    }
                    else
                    {
                        _logger.LogError("Database cleanup request failed.");
                    }
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Failed to connect to API.");
            }

            bool deauthenticationSuccessful = await _authenticationService.Deauthenticate(token);
            if (deauthenticationSuccessful == false)
            {
                _logger.LogError("Deauthentication request failed.");
                return false;
            }

            return true;
        }
    }
}
