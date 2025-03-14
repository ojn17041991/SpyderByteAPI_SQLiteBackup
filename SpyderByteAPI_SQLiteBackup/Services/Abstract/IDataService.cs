namespace SpyderByteAPI_SQLiteBackup.Services.Abstract
{
    public interface IDataService
    {
        Task<bool> RequestBackup();

        Task<bool> RequestCleanup();
    }
}
