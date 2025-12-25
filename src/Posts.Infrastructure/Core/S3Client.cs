using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.DependencyInjection;
using Posts.Application.Core;
using Posts.Infrastructure.Core.Models;

namespace Posts.Infrastructure.Core
{
    public class S3Client : IS3Client
    {
        private readonly IAmazonS3 _amazonS3;
        private readonly IAmazonS3 _signerClient;
        private readonly S3Options _options;

        private const string TempPrefix = "temp/";

        public S3Client(
            IAmazonS3 amazonS3,
            S3Options options,
            [FromKeyedServices(DependencyInjectionTokens.S3_CLIENT_SIGNER)] IAmazonS3 signerClient
        )
        {
            _amazonS3 = amazonS3;
            _signerClient = signerClient;
            _options = options;
        }

        public async Task ConfigureCleanupAsync()
        {
            var lifecycleConfig = new LifecycleConfiguration
            {
                Rules = new List<LifecycleRule>
                {
                    new LifecycleRule
                    {
                        Id = "DeleteTemp",
                        Filter = new LifecycleFilter { LifecycleFilterPredicate = new LifecyclePrefixPredicate { Prefix = "temp/" } },
                        Status = LifecycleRuleStatus.Enabled,
                        Expiration = new LifecycleRuleExpiration { Days = 1 }
                    }
                }
            };

            await _amazonS3.PutLifecycleConfigurationAsync(new PutLifecycleConfigurationRequest
            {
                BucketName = _options.BucketName,
                Configuration = lifecycleConfig
            });
        }

        public string GetPublicUrl(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            return $"{_options.PublicDomain.TrimEnd('/')}/{_options.BucketName}/{key}";
        }

        public string GetPresignedUrl(string key, int? expiresMinutes = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var protocol = _options.PublicDomain.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ?
                Protocol.HTTP :
                Protocol.HTTPS;

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(expiresMinutes ?? 60),
                Protocol = protocol
            };

            return _signerClient.GetPreSignedURL(request);
        }

        public async Task<string> UploadAsync(string key, Stream stream, string contentType, bool isPublic = false)
        {
            await UploadCoreAsync(key, stream, contentType, isPublic);
            return key;
        }

        public async Task<string> UploadTempAsync(string fileName, Stream stream, string contentType)
        {
            var extension = Path.GetExtension(fileName);
            var key = $"{TempPrefix}{Guid.NewGuid()}{extension}";
            await UploadCoreAsync(key, stream, contentType, isPublic: false);
            return key;
        }

        public async Task<string> PersistFileAsync(string key, string targetFolder, bool makePublic = false)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!key.StartsWith(TempPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return key;
            }

            var fileName = Path.GetFileName(key);
            var cleanTarget = targetFolder.TrimEnd('/').TrimStart('/');
            var newKey = string.IsNullOrEmpty(cleanTarget)
                ? fileName
                : $"{cleanTarget}/{fileName}";

            var copyRequest = new CopyObjectRequest
            {
                SourceBucket = _options.BucketName,
                SourceKey = key,
                DestinationBucket = _options.BucketName,
                DestinationKey = newKey,
                CannedACL = makePublic ? S3CannedACL.PublicRead : S3CannedACL.Private
            };

            await _amazonS3.CopyObjectAsync(copyRequest);

            return newKey;
        }

        public async Task DeleteFileAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            await _amazonS3.DeleteObjectAsync(_options.BucketName, key);
        }

        private async Task UploadCoreAsync(string key, Stream stream, string contentType, bool isPublic)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                InputStream = stream,
                ContentType = contentType,
                CannedACL = isPublic ? S3CannedACL.PublicRead : S3CannedACL.Private
            };

            await _amazonS3.PutObjectAsync(putRequest);
        }
    }
}
