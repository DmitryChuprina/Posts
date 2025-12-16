namespace Posts.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public IReadOnlyDictionary<string, string[]>? Errors { get; }

        public ValidationException(string? message) : base(message)
        { }

        public ValidationException(
            string? message,
            IReadOnlyDictionary<string, string[]>? errors
        ) : base(message)
        {
            Errors = errors;
        }
    }
}
