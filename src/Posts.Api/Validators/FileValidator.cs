using FluentValidation;
using Posts.Contract.Models;
using Posts.Domain.Utils;

namespace Posts.Api.Validators
{
    public class FileValidator : AbstractValidator<FileDto>
    {
        public FileValidator()
        {
            RuleFor(x => x.Key)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(Validation.FILE_KEY_MAX_LENGTH)
                .WithMessage(ValidatorsConstants.MAX_LENGTH_MESSAGE);
        }
    }
}
