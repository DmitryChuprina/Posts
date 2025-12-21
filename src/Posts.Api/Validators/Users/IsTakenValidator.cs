using FluentValidation;
using Posts.Contract.Models.Users;
using Posts.Domain.Utils;

namespace Posts.Api.Validators.Users
{
    public class EmailIsTakenValidator : AbstractValidator<EmailIsTakenDto>
    {
        public EmailIsTakenValidator()
        {
            RuleFor(c => c.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(Validation.IsEmail)
                .WithMessage(ValidatorsConstants.EMAIL_MESSAGE)
                .MaximumLength(Validation.DEFAULT_STRING_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE);
        }
    }

    public class UsernameIsTakenValidator : AbstractValidator<UsernameIsTakenDto>
    {
        public UsernameIsTakenValidator()
        {
            RuleFor(c => c.Username)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(Validation.IsUsername)
                .WithMessage(ValidatorsConstants.USERNAME_MESSAGE)
                .MaximumLength(Validation.DEFAULT_STRING_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE);
        }
    }
}
