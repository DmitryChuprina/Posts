using Amazon.Runtime;
using Amazon.S3;
using Posts.Infrastructure.Core.Models;

namespace Posts.Infrastructure.Extensions
{
    public static class S3Extensions
    {
        public static AmazonS3Client CreateAmazonS3Client(this S3Options opts, bool isOverrideUrl = false)
        {
            var serviceUrl = opts.ServiceUrl;

            if (isOverrideUrl && !string.IsNullOrEmpty(opts.PublicDomain))
            {
                var newUrl = opts.PublicDomain;
                var isUri = Uri.TryCreate(newUrl, UriKind.Absolute, out var uri);

                if (isUri)
                {
                    serviceUrl = $"{uri!.Scheme}://{uri.Authority}";
                }
                if (!isUri)
                {
                    serviceUrl = newUrl;
                }
            }

            var useHttp = serviceUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase);

            var config = new AmazonS3Config
            {
                ServiceURL = serviceUrl,
                ForcePathStyle = opts.ForcePathStyle,
                UseHttp = useHttp
            };

            var credentials = new BasicAWSCredentials(opts.AccessKey, opts.SecretKey);

            return new AmazonS3Client(credentials, config);
        }
    }
}
