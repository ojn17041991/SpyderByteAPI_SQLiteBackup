using SpyderByteAPI_SQLiteBackup.Services.Abstract;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Net.Http.Headers;

namespace SpyderByteAPI_SQLiteBackup.Services
{
    public class HttpService : IHttpService
    {
        private ILogger<HttpService> _logger;   
        private IHttpClientFactory _httpClientFactory;

        private string _user;
        private string _secret;
        private string _url;
        private string _authenticationEndpoint;
        private string _databaseBackupEndpoint;

        private string _token = string.Empty;

        public HttpService(IHttpClientFactory httpClientFactory, ILogger<HttpService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            _user = Environment.GetEnvironmentVariable("User") ?? string.Empty;
            _secret = Environment.GetEnvironmentVariable("Secret") ?? string.Empty;
            _url = Environment.GetEnvironmentVariable("Url") ?? string.Empty;
            _authenticationEndpoint = Environment.GetEnvironmentVariable("AuthenticationEndpoint") ?? string.Empty;
            _databaseBackupEndpoint = Environment.GetEnvironmentVariable("DatabaseBackupEndpoint") ?? string.Empty;
        }

        public async Task<bool> RequestBackup()
        {
            _logger.LogInformation($"Database backup requested using (UserName={_user},Secret=xxxxxx,Url={_url},AuthenticationEndpoint={_authenticationEndpoint},DatabaseBackupEndpoint={_databaseBackupEndpoint}).");

            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    // Authenticate and get token.
                    var credentials = new Dictionary<string, string>
                    {
                        { "userName", _user },
                        { "password", _secret }
                    };
                    var credentialsJson = JsonSerializer.Serialize(credentials);
                    var credentialsContent = new StringContent(credentialsJson, Encoding.UTF8, "application/json");
                    if (credentialsContent == null)
                    {
                        _logger.LogError("Failed to convert API credentials to HTTP content.");
                        return false;
                    }

                    var authenticationResponse = await httpClient.PostAsync(_url + _authenticationEndpoint, credentialsContent);
                    if (authenticationResponse.IsSuccessStatusCode)
                    {
                        var responseJson = await authenticationResponse.Content.ReadAsStringAsync();
                        if (responseJson != null)
                        {
                            _token = responseJson;
                        }
                        _logger.LogInformation("Authentication request successful.");
                    }
                    else
                    {
                        _logger.LogError("Authentication request failed.");
                        return false;
                    }

                    // Make request for DB backup.
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                    var databaseBackupResponse = await httpClient.PostAsync(_url + _databaseBackupEndpoint, null);
                    if (databaseBackupResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Database backup request successful.");
                    }
                    else
                    {
                        _logger.LogError("Database backup request failed.");
                        // Don't return- continue to deauthentication.
                    }

                    // Deauthenticate.
                    var deauthenticationResponse = await httpClient.DeleteAsync(_url + _authenticationEndpoint);
                    if (deauthenticationResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Deauthentication request successful.");
                    }
                    else
                    {
                        _logger.LogError("Deauthentication request failed.");
                    }
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError("Failed to connect to API.", hre);
                return false;
            }

            _token = string.Empty;
            return true;
        }
    }
}
