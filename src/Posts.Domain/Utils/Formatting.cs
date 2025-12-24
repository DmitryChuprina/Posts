namespace Posts.Domain.Utils
{
    public static class Formatting
    {
        public static string? NullableDefaultString(string? value) =>
            !string.IsNullOrEmpty(value) ?
                value.Trim() :
                null;

        public static string DefaultString(string value) =>
           !string.IsNullOrEmpty(value) ?
                value.Trim() :
                "";

        public static string Email(string value) =>
            DefaultString(value).ToLower();

        public static string Username(string value) =>
            DefaultString(value).ToLower();

        public static string EmailOrUsername(string value) =>
            DefaultString(value).ToLower();

        public static string Password(string value) =>
            DefaultString(value);

        public static string? NullablePassword(string? value) =>
            NullableDefaultString(value);

        public static string Tag(string value) =>
            DefaultString(value).ToLowerInvariant();
    }
}
