using Amazon.S3;
using Amazon.S3.Util;
using FluentAssertions;
using Moq;
using Posts.Infrastructure.Core;
using Posts.Infrastructure.Core.Models;
using Posts.Infrastructure.Extensions;
using Posts.Infrastructure.IntegrationTests.Fixtures;
using System.Text;

namespace Posts.Infrastructure.IntegrationTests.Core
{
    [Collection("IntegrationTests")]
    public class S3ClientTests : IAsyncLifetime
    {
        private readonly TestInfrastructure _infra;

        private readonly S3Options _options = null!;
        private readonly IAmazonS3 _signer = null!;
        private readonly IAmazonS3 _amazonS3 = null!;
        private readonly S3Client _sut = null!;

        private const string TestBucket = "integration-test-bucket";

        public S3ClientTests(TestInfrastructure infra)
        {
            _infra = infra;
            var publicPort = _infra.Minio.GetMappedPublicPort(9000);
            var hostBaseUrl = $"http://localhost:{publicPort}";
            _options = new S3Options
            {
                BucketName = TestBucket,
                ServiceUrl = _infra.Minio.GetConnectionString(),
                ForcePathStyle = true,
                AccessKey = "minioadmin",
                SecretKey = "minioadmin",
                PublicDomain = hostBaseUrl
            };
            _amazonS3 = _options.CreateAmazonS3Client();
            _signer = _options.CreateAmazonS3Client(true);
            _sut = new S3Client(
                _amazonS3,
                _options,
                _signer
            );
        }

        public async Task InitializeAsync()
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_amazonS3, TestBucket);
            if (!bucketExists)
            {
                await _amazonS3.PutBucketAsync(TestBucket);
            }
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Theory]
        [InlineData("my-key.jpg")]
        [InlineData("folder/file.png")]
        [InlineData("file")]
        public async Task GetPublicUrl_Should_Format_Correctly(string key)
        {
            var content = "Public Content";
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            await _sut.UploadAsync(key, stream, "text/plain", true);

            // Act
            var url = _sut.GetPublicUrl(key);

            // Assert
            url.Should().NotBeNullOrEmpty();

            var expectedBase = _options.PublicDomain;
            url.Should().StartWith(expectedBase);

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);

            response.IsSuccessStatusCode.Should().BeTrue($"Public URL should be accessible. Url: {url}");

            var downloadedContent = await response.Content.ReadAsStringAsync();
            downloadedContent.Should().Be(content);
        }

        [Fact]
        public async Task GetPresignedUrl_Should_Return_Accessible_Url()
        {
            // Arrange
            var key = "test-presigned.txt";
            var content = "Secret Content";
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            await _sut.UploadAsync(key, stream, "text/plain");

            // Act
            var url = _sut.GetPresignedUrl(key, expiresMinutes: 10);

            // Assert
            url.Should().NotBeNullOrEmpty();

            var expectedBase = _options.PublicDomain;
            url.Should().StartWith(expectedBase);

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);

            response.IsSuccessStatusCode.Should().BeTrue($"Presigned URL should be accessible. Url: {url}");

            var downloadedContent = await response.Content.ReadAsStringAsync();
            downloadedContent.Should().Be(content);
        }

        [Fact]
        public async Task Upload_WithPrivateAcl_ShouldReturn403Forbidden()
        {
            // Arrange
            var key = $"private-file-{Guid.NewGuid()}.txt";
            var content = "This is Top Secret Data";
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            // Act
            var fileKey = await _sut.UploadAsync(key, stream, "text/plain", isPublic: false);
            var fileUrl = _sut.GetPublicUrl(fileKey);

            // Assert
            using var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(fileUrl);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
            var exists = await AmazonS3Util.DoesS3BucketExistV2Async(_amazonS3, _options.BucketName);
        }

        [Fact]
        public async Task UploadAsync_Should_Put_File_In_Bucket()
        {
            // Arrange
            var key = "test-folder/hello.txt";
            var content = "Hello World";
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            // Act
            await _sut.UploadAsync(key, stream, "text/plain");

            // Assert
            var response = await _amazonS3.GetObjectAsync(TestBucket, key);
            using var reader = new StreamReader(response.ResponseStream);
            var downloadedContent = await reader.ReadToEndAsync();

            downloadedContent.Should().Be(content);
            response.Headers.ContentType.Should().Be("text/plain");
        }

        [Fact]
        public async Task PersistFileAsync_Should_Move_File_From_Temp_To_Target()
        {
            // Arrange
            var fileName = "image.png";
            using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

            var tempKey = await _sut.UploadTempAsync(fileName, stream, "image/png");

            var tempExists = await FileExistsInS3(tempKey);
            tempExists.Should().BeTrue("Temp file should exist before move");

            // Act
            var targetFolder = "users/avatars";
            var newKey = await _sut.PersistFileAsync(tempKey, targetFolder, makePublic: true);

            // Assert
            newKey.Should().Be($"users/avatars/{Path.GetFileName(tempKey)}");
            var newExists = await FileExistsInS3(newKey);
            newExists.Should().BeTrue("Target file should exist");
        }

        [Fact]
        public async Task ConfigureCleanupAsync_Should_Apply_Lifecycle_Rule()
        {
            // Act
            await _sut.ConfigureCleanupAsync();

            // Assert
            var lifecycle = await _amazonS3.GetLifecycleConfigurationAsync(TestBucket);

            lifecycle.Configuration.Rules.Should().ContainSingle();
            var rule = lifecycle.Configuration.Rules[0];

            rule.Id.Should().Be("DeleteTemp");
            rule.Status.Should().Be(LifecycleRuleStatus.Enabled);
            rule.Expiration.Days.Should().Be(1);
        }

        [Fact]
        public async Task DeleteFileAsync_Should_Remove_File()
        {
            // Arrange
            var key = "to-delete.txt";
            using var stream = new MemoryStream(new byte[1]);
            await _sut.UploadAsync(key, stream, "text/plain");

            (await FileExistsInS3(key)).Should().BeTrue();

            // Act
            await _sut.DeleteFileAsync(key);

            // Assert
            (await FileExistsInS3(key)).Should().BeFalse();
        }

        private async Task<bool> FileExistsInS3(string key)
        {
            try
            {
                await _amazonS3.GetObjectMetadataAsync(TestBucket, key);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
