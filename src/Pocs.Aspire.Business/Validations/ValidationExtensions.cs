using FluentValidation;
using Pocs.Aspire.Domain.Users.ValueObjects;
using System;

namespace Pocs.Aspire.Business.Validations;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, Guid> ValidUserId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Id is required and must be a valid guid format.");
    }
    public static IRuleBuilderOptions<T, string> ValidFirstName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");
    }
    public static IRuleBuilderOptions<T, string> ValidLastName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");
    }

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(Email.IsValid).WithMessage("A valid email is required.")
            .MaximumLength(100).WithMessage("Emails cannot exceed 100 characters.");
    }

    public static IRuleBuilderOptions<T, int> ValidPage<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(1).WithMessage("Page must be 1 or greater.");
    }

    public static IRuleBuilderOptions<T, int> ValidPageSize<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
