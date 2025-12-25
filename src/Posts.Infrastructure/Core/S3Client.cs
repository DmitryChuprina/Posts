using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.DependencyInjection;
using Posts.Application.Core;
using Posts.Infrastructure.Core.Models;

namespace Posts.Infrastructure.Core
{
    internal enum StorageScope
    {
        Private,
        Public,
        Temp
    }

    public class S3Client : IS3Client
    {
        private readonly IAmazonS3 _amazonS3;
        private readonly IAmazonS3 _signerClient;
        private readonly S3Options _options;

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

        public async Task ConfigureBucketAsync()
        {
            var bucketName = _options.BucketName;

            if (!await AmazonS3Util.DoesS3BucketExistV2Async(_amazonS3, bucketName))
            {
                await _amazonS3.PutBucketAsync(bucketName);
            }

            var policyJson = $@"
                {{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {{
                            ""Sid"": ""AllowPublicAccess"",
                            ""Effect"": ""Allow"",
                            ""Principal"": ""*"",
                            ""Action"": [""s3:GetObject""],
                            ""Resource"": [
                                ""arn:aws:s3:::{bucketName}/{GetKeyPrefix(StorageScope.Public)}*""
                            ]
                        }}
                    ]
                }}";

            await _amazonS3.PutBucketPolicyAsync(new PutBucketPolicyRequest
            {
                BucketName = bucketName,
                Policy = policyJson
            });

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

        public Task<string> UploadAsync(string key, Stream stream, string contentType, bool isPublic = false)
        {
            return UploadCoreAsync(key, stream, contentType, isPublic ? StorageScope.Public : StorageScope.Private);
        }

        public Task<string> UploadTempAsync(string fileName, Stream stream, string contentType)
        {
            return UploadCoreAsync(fileName, stream, contentType, StorageScope.Temp);
        }

        public async Task<string> PersistFileAsync(string key, string targetFolder, bool makePublic = false)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var tempPrefix = GetKeyPrefix(StorageScope.Temp);

            if (!key.StartsWith(tempPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return key;
            }

            var fileName = Path.GetFileName(key);
            var cleanTarget = targetFolder.TrimEnd('/').TrimStart('/');
            var newKeyPart = string.IsNullOrEmpty(cleanTarget) ?
                fileName :
                $"{cleanTarget}/{fileName}";

            var newPrefix = makePublic ?
                GetKeyPrefix(StorageScope.Public) :
                GetKeyPrefix(StorageScope.Private);

            var newKey = $"{newPrefix}{newKeyPart}";

            var copyRequest = new CopyObjectRequest
            {
                SourceBucket = _options.BucketName,
                SourceKey = key,
                DestinationBucket = _options.BucketName,
                DestinationKey = newKey
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

        private string GetKeyPrefix(StorageScope scope)
        {
            return scope switch
            {
                StorageScope.Public => "public/",
                StorageScope.Temp => "temp/",
                StorageScope.Private => "private/",
                _ => "private/"
            };
        }

        private async Task<string> UploadCoreAsync(string key, Stream stream, string contentType, StorageScope scope)
        {
            var prefix = GetKeyPrefix(scope);

            key = $"{prefix}{key}";

            var putRequest = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                InputStream = stream,
                ContentType = contentType
            };

            await _amazonS3.PutObjectAsync(putRequest);

            return key;
        }
    }
}
