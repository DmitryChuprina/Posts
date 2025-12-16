using System.Text.RegularExpressions;

namespace Posts.Domain.Utils
{
    public static class Validation
    {
        private static readonly Regex EmailRegex =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex UsernameRegex =
            new(@"^[A-Za-z0-9_]{3,24}$", RegexOptions.Compiled);

        private static readonly Regex PasswordRegex = new(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            RegexOptions.Compiled
        );

        public const int DEFAULT_STRING_MAX_LENGTH = 255;

        public static bool IsName(string value) =>
            !string.IsNullOrEmpty(value) && value.All(Char.IsLetter);

        public static bool IsEmail(string value) =>
            !string.IsNullOrEmpty(value) && EmailRegex.IsMatch(value);

        public static bool IsUsername(string value) =>
            !string.IsNullOrEmpty(value) && UsernameRegex.IsMatch(value);

        public static bool IsEmailOrUsername(string value) =>
            IsEmail(value) || IsUsername(value);

        public static bool IsPassword(string value) =>
            !string.IsNullOrEmpty(value) && PasswordRegex.IsMatch(value);
    }
}
