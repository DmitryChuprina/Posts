using FluentValidation;
using Posts.Contract.Models.Auth;
using Posts.Domain.Utils;

namespace Posts.Api.Validators.Auth
{
    public class SignUpValidator : AbstractValidator<SignUpRequestDto>
    {
        public SignUpValidator()
        {
            RuleFor(c => c.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(Validation.IsEmail)
                .WithMessage(ValidatorsConstants.EMAIL_MESSAGE)
                .MaximumLength(Validation.DEFAULT_STRING_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE);

            RuleFor(c => c.Username)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(Validation.IsUsername)
                .WithMessage(ValidatorsConstants.USERNAME_MESSAGE)
                .MaximumLength(Validation.DEFAULT_STRING_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE);

            RuleFor(c => c.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(Validation.IsPassword)
                .WithMessage(ValidatorsConstants.PASSWORD_MESSAGE)
                .MaximumLength(Validation.DEFAULT_STRING_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE);
        }
    }
}
