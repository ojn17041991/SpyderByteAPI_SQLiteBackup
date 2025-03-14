using SpyderByteAPI_SQLiteBackup.Services.Abstract;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SpyderByteAPI_SQLiteBackup.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private ILogger<AuthenticationService> _logger;
        private IHttpClientFactory _httpClientFactory;

        private string _user;
        private string _secret;
        private string _url;
        private string _authenticationEndpoint;

        public AuthenticationService(IHttpClientFactory httpClientFactory, ILogger<AuthenticationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            _user = Environment.GetEnvironmentVariable("User") ?? string.Empty;
            _secret = Environment.GetEnvironmentVariable("Secret") ?? string.Empty;
            _url = Environment.GetEnvironmentVariable("Url") ?? string.Empty;
            _authenticationEndpoint = Environment.GetEnvironmentVariable("Authentication:Endpoint") ?? string.Empty;
        }

        public async Task<string?> Authenticate()
        {
            _logger.LogInformation($"Authentication requested using (UserName={_user},Secret=xxxxxx,Url={_url},Endpoint={_authenticationEndpoint}).");

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
                        return null;
                    }

                    var authenticationResponse = await httpClient.PostAsync(_url + _authenticationEndpoint, credentialsContent);
                    if (authenticationResponse.IsSuccessStatusCode)
                    {
                        var token = await authenticationResponse.Content.ReadAsStringAsync();
                        if (token != null)
                        {
                            _logger.LogInformation("Authentication request successful.");
                            return token;
                        }
                        else
                        {
                            _logger.LogError("Authentication request failed.");
                            return null;
                        }
                    }
                    else
                    {
                        _logger.LogError("Authentication request failed.");
                        return null;
                    }
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Failed to connect to API.");
                return null;
            }
        }

        public async Task<bool> Deauthenticate(string token)
        {
            _logger.LogInformation($"Deuthentication requested using (UserName={_user},Secret=xxxxxx,Url={_url},Endpoint={_authenticationEndpoint}).");

            try
            {
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var deauthenticationResponse = await httpClient.DeleteAsync(_url + _authenticationEndpoint);
                    if (deauthenticationResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Deauthentication request successful.");
                        return true;
                    }
                    else
                    {
                        _logger.LogError("Deauthentication request failed.");
                        return false;
                    }
                }
            }
            catch (HttpRequestException hre)
            {
                _logger.LogError(hre, "Failed to connect to API.");
                return false;
            }
        }
    }
}
