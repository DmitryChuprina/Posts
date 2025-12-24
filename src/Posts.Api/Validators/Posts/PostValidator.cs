using FluentValidation;
using Posts.Contract.Models.Posts;
using Posts.Domain.Utils;

namespace Posts.Api.Validators.Posts
{
    public class CreatePostValidator : AbstractValidator<CreatePostDto>
    {
        public CreatePostValidator()
        {
            RuleFor(c => c.Content)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(Validation.POST_CONTENT_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE);
        }
    }

    public class UpdatePostValidator : AbstractValidator<UpdatePostDto>
    {
        public UpdatePostValidator()
        {
            RuleFor(c => c.Content)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(Validation.POST_CONTENT_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE);
        }
    }
}
