namespace Posts.Api.Validators
{
    public static class ValidatorsConstants
    {
        public const string EMPTY_MESSAGE = "Property can't be empty";
        public const string MAX_LENGTH_MESSAGE = "Property must have maximum {MaxLength} length";

        public const string EMAIL_OR_USERNAME_MESSAGE = "Property must be a valid email or username";

        // TODO: Need more info
        public const string EMAIL_MESSAGE = "Property must be a valid email";
        public const string USERNAME_MESSAGE = "Property must be a valid username";
        public const string PASSWORD_MESSAGE = "Property must be a valid email";
    }
}
