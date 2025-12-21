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

        public static async Task CleanupOldFileAsync(
            this IS3Client client,
            string? oldKey,
            string? newKey,
            ILogger? logger = null
        )
        {
            if (string.IsNullOrEmpty(oldKey))
            {
                return;
            }

            if (string.Equals(oldKey, newKey, StringComparison.Ordinal))
            {
                return;
            }

            try
            {
                await client.DeleteFileAsync(oldKey);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to cleanup old S3 file: {Key}. Manual cleanup required.", oldKey);
            }
        }
    }
}
