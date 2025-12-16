using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Posts.Api.Filters
{
    public sealed class ValidationFilter : IActionFilter
    {
        private readonly IServiceProvider _sp;

        public ValidationFilter(IServiceProvider sp)
        {
            _sp = sp;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                throw new Application.Exceptions.ValidationException("Validation failed: Invalid model");
            }

            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg is null)
                {
                    continue;
                }

                var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
                if (_sp.GetService(validatorType) is not IValidator validator)
                {
                    continue;
                }

                var result = validator.Validate(new ValidationContext<object>(arg));

                if (!result.IsValid)
                {
                    var errors = result.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => ToCamelCase(g.Key),
                                g => g.Select(e => e.ErrorMessage).ToArray()
                            );

                    throw new Application.Exceptions.ValidationException(
                        BuildMessageByErrors(errors),
                        errors
                    );
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }

        private string BuildMessageByErrors(IReadOnlyDictionary<string, string[]> errors)
        {
            return "Validation failed in properties: " + string.Join(", ", errors.Keys);
        }

        private string ToCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}
