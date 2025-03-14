namespace SpyderByteAPI_SQLiteBackup.Services.Abstract
{
    public interface IAuthenticationService
    {
        public Task<string?> Authenticate();

        public Task<bool> Deauthenticate(string token);
    }
}
