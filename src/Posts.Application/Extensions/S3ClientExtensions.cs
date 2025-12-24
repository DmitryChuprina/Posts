using Microsoft.Extensions.Logging;
using Posts.Application.Core;
using Posts.Contract.Models;

namespace Posts.Application.Extensions
{
    public static class S3ClientExtensions
    {
        public static FileDto? GetPublicFileDto(this IS3Client client, string? key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            return new FileDto
            {
                Key = key,
                Url = client.GetPublicUrl(key)
            };
        }

        public static FileDto? GetPresignedFileDto(this IS3Client client, string? key, int? expiresMinutes = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            return new FileDto
            {
                Key = key,
                Url = client.GetPresignedUrl(key, expiresMinutes)
            };
        }

        public static async Task<string?> PersistFileDtoAsync(
            this IS3Client client,
            FileDto? dto,
            string targetFolder,
            bool makePublic = false
        )
        {
            if (dto is null || string.IsNullOrEmpty(dto.Key))
            {
                return null;
            }

            var persistedKey = await client.PersistFileAsync(dto.Key, targetFolder, makePublic);

            return persistedKey;
        }

        public static async Task CleanupPersistedFilesAsync(this IS3Client client, IEnumerable<string> persistedFilesKeys, ILogger? logger = null)
        {
            if (!persistedFilesKeys.Any())
            {
                return;
            }

            try
            {
                await Task.WhenAll(
                    persistedFilesKeys.Select(key => 
                        key is not null 
                            ? Task.Run(() => client.DeleteFileAsync(key)) 
                            : Task.CompletedTask
                    )
                );
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to cleanup persisted S3 files. Manual cleanup required.");
            }
        }
    }
}
