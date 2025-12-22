using FluentValidation;
using Posts.Contract.Models.Users;
using Posts.Domain.Utils;

namespace Posts.Api.Validators.Users
{
    public class UserProfileValidator : AbstractValidator<UpdateUserProfileDto>
    {
        public UserProfileValidator()
        {
            RuleFor(c => c.FirstName)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(Validation.DEFAULT_STRING_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE)
                .MinimumLength(Validation.MIN_NAME_LENGTH)
                .WithMessage(ValidatorsConstants.MIN_LENGTH_MESSAGE)
                .Must(Validation.IsName)
                .WithMessage(ValidatorsConstants.PART_OF_NAME_MESSAGE)
                .When(x => x.FirstName != null);

            RuleFor(c => c.LastName)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(Validation.DEFAULT_STRING_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE)
                .MinimumLength(Validation.MIN_NAME_LENGTH)
                .WithMessage(ValidatorsConstants.MIN_LENGTH_MESSAGE)
                .Must(Validation.IsName)
                .WithMessage(ValidatorsConstants.PART_OF_NAME_MESSAGE)
                .When(x => x.LastName != null);

            RuleFor(c => c.Username)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(Validation.IsUsername)
                .WithMessage(ValidatorsConstants.USERNAME_MESSAGE)
                .MaximumLength(Validation.DEFAULT_STRING_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE);

            RuleFor(c => c.Description)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(Validation.USER_DESCRIPTION_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE)
                .When(x => x.Description != null);
        }
    }
}
