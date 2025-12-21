using Posts.Application.Core;
using Posts.Application.Core.Models;
using Posts.Domain.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace Posts.Infrastructure.Core
{
    internal class FileOptimizer : IFileOptimizer
    {
        private readonly HashSet<string> _imageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".bmp", ".webp"
        };

        public async Task<FileOptimizerResult> ProcessAsync(Stream input, string originalFileName, UploadType type)
        {
            var ext = Path.GetExtension(originalFileName);

            if (_imageExtensions.Contains(ext))
            {
                return await OptimizeImageAsync(input, originalFileName, type);
            }

            return new FileOptimizerResult(
                input,
                originalFileName,
                GetContentType(ext),
                input.Length
            );
        }

        private async Task<FileOptimizerResult> OptimizeImageAsync(Stream input, string fileName, UploadType type)
        {
            input.Position = 0;
            using var image = await Image.LoadAsync(input);

            switch (type)
            {
                case UploadType.ProfileImage:
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(400, 400),
                        Mode = ResizeMode.Crop
                    }));
                    break;
                case UploadType.ProfileBanner:
                    if (image.Width > 1920)
                    {
                        image.Mutate(x => x.Resize(1920, 0));
                    }
                    break;
            }

            var outStream = new MemoryStream();
            await image.SaveAsWebpAsync(outStream, new WebpEncoder { Quality = 80 });
            outStream.Position = 0;

            return new FileOptimizerResult
            (
                outStream,
                Path.ChangeExtension(fileName, ".webp"),
                "image/webp",
                outStream.Length
            );
        }

        private string GetContentType(string ext) => ext.ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".mp4" => "video/mp4",
            _ => "application/octet-stream"
        };
    }
}
