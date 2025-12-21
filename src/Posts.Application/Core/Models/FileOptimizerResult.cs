namespace Posts.Application.Core.Models
{
    public class FileOptimizerResult
    {
        public FileOptimizerResult(Stream stream, string fileName, string contentType, long size)
        {
            Stream = stream;
            FileName = fileName;
            ContentType = contentType;
            Size = size;
        }

        public Stream Stream { get; }
        public string FileName { get; }
        public string ContentType { get; }
        public long Size { get;  }
    }
}
