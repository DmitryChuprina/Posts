namespace Posts.Application.Core
{
    public interface IS3Client
    {
        Task ConfigureBucketAsync();
        string GetPublicUrl(string key);
        string GetPresignedUrl(string key, int? expiresMinutes = null);
        Task<string> UploadAsync(string key, Stream stream, string contentType, bool isPublic = false);
        Task<string> UploadTempAsync(string fileName, Stream stream, string contentType);
        Task<string> PersistFileAsync(string tempKey, string targetFolder, bool makePublic = false);
        Task DeleteFileAsync(string key);
    }
}
