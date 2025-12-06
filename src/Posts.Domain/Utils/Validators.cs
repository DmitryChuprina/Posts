using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Posts.Domain.Utils
{
    public static class Validators
    {
        private static readonly Regex EmailRegex =
            new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex UsernameRegex =
            new(@"^[A-Za-z0-9_]{3,24}$", RegexOptions.Compiled);

        public static bool IsEmail(string value) =>
            EmailRegex.IsMatch(value);

        public static bool IsUsername(string value) =>
            UsernameRegex.IsMatch(value);
    }
}
