using FluentValidation;
using Pocs.Aspire.Domain.Users.ValueObjects;
using System;

namespace Pocs.Aspire.Business.Validations;

public static class ValidationExtensions
{
    private const int MaxPage = 1_000_000;

    public static IRuleBuilderOptions<T, Guid> ValidUserId<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Id is required and must be a valid guid format.");
    }
    public static IRuleBuilderOptions<T, string> ValidFirstName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(FirstName.IsNotBlank).WithMessage(FirstName.RequiredMessage)
            .Must(FirstName.IsWithinMaxLength).WithMessage(FirstName.TooLongMessage);
    }
    public static IRuleBuilderOptions<T, string> ValidLastName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(LastName.IsNotBlank).WithMessage(LastName.RequiredMessage)
            .Must(LastName.IsWithinMaxLength).WithMessage(LastName.TooLongMessage);
    }

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(Email.IsValid).WithMessage(Email.InvalidMessage)
            .Must(Email.IsWithinMaxLength).WithMessage(Email.TooLongMessage);
    }

    public static IRuleBuilderOptions<T, int> ValidPage<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(1).WithMessage("Page must be 1 or greater.")
            .LessThanOrEqualTo(MaxPage).WithMessage($"Page must be {MaxPage} or less.");
    }

    public static IRuleBuilderOptions<T, int> ValidPageSize<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
    }
}
