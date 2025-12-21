using FluentValidation;
using Posts.Contract.Models.Auth;

namespace Posts.Api.Validators.Auth
{
    public class AuthTokensValidator : AbstractValidator<AuthTokensDto>
    {
        public AuthTokensValidator()
        {
            RuleFor(c => c.AccessToken)
                .NotEmpty()
                .WithMessage(ValidatorsConstants.EMPTY_MESSAGE);

            RuleFor(c => c.RefreshToken)
                .NotEmpty()
                .WithMessage(ValidatorsConstants.EMPTY_MESSAGE);
        }
    }
}
