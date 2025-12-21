using Microsoft.AspNetCore.Mvc;
using Posts.Application.Core;
using Posts.Application.Extensions;
using Posts.Contract.Models;
using Posts.Domain.Enums;

namespace Posts.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IS3Client _s3Client;
        private readonly IFileOptimizer _fileOptimizer;

        public FilesController(
            IS3Client s3Client,
            IFileOptimizer fileOptimizer
        ) {
            _s3Client = s3Client;
            _fileOptimizer = fileOptimizer;
        }

        [HttpPost("upload")]
        public async Task<FileDto> Upload(
            IFormFile file,
            [FromQuery] UploadType type = UploadType.Default
        ){
            using var rawStream = file.OpenReadStream();
            var processedFile = await _fileOptimizer.ProcessAsync(rawStream, file.FileName, type);
            var key = await _s3Client.UploadTempAsync(
                processedFile.FileName,
                processedFile.Stream,
                processedFile.ContentType
            );
            return _s3Client.GetPresignedFileDto(key)!;
        }
    }
}
