namespace SpyderByteAPI_SQLiteBackup.Services.Abstract
{
    public interface IHttpService
    {
        Task<bool> RequestBackup();
    }
}
