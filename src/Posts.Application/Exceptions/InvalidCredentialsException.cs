namespace Posts.Application.Exceptions
{
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string? message = null) : base(message ?? "Invalid credentials")
        {
        }
    }
}
