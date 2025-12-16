using FluentValidation;
using Posts.Contract.Models.Auth;
using Posts.Domain.Utils;

namespace Posts.Api.Validators.Auth
{
    public class SignInValidator : AbstractValidator<SignInRequestDto>
    {
        public SignInValidator()
        {
            RuleFor(c => c.EmailOrUsername)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(ValidatorsConstants.EMPTY_MESSAGE)
                .MaximumLength(Validation.DEFAULT_STRING_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE)
                .Must(Validation.IsEmailOrUsername)
                .WithMessage(ValidatorsConstants.EMAIL_OR_USERNAME_MESSAGE);

            RuleFor(c => c.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(ValidatorsConstants.EMPTY_MESSAGE)
                .MaximumLength(Validation.DEFAULT_STRING_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE);
        }
    }
}
