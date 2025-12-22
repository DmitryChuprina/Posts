using FluentValidation;
using Posts.Contract.Models.Users;
using Posts.Domain.Utils;

namespace Posts.Api.Validators.Users
{
    public class UserSecurityValidator : AbstractValidator<UpdateUserSecurityDto>
    {
        public UserSecurityValidator()
        {
            RuleFor(c => c.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(ValidatorsConstants.EMPTY_MESSAGE)
                .MaximumLength(Validation.DEFAULT_STRING_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE)
                .When(x => x.Password != null);

            RuleFor(c => c.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(Validation.IsEmail)
                .WithMessage(ValidatorsConstants.EMAIL_MESSAGE)
                .MaximumLength(Validation.DEFAULT_STRING_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE);
        }
    }
}
