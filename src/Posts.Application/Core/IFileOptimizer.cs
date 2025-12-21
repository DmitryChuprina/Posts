using Posts.Application.Core.Models;
using Posts.Domain.Enums;

namespace Posts.Application.Core
{
    public interface IFileOptimizer
    {
        Task<FileOptimizerResult> ProcessAsync(Stream input, string originalFileName, UploadType type);
    }
}
