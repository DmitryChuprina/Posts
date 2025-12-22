namespace Posts.Application.Exceptions
{
    public class ConcurencyException : Exception
    {
        public ConcurencyException(string? message = null) : base(message)
        {
        }
    }
}
