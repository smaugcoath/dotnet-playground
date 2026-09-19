namespace Pocs.Aspire.Business.Tests.Unit.Validations;

using FluentValidation;
using Pocs.Aspire.Business.Validations;
using Shouldly;

/// <summary>
/// Covers the divergence between FluentValidation's <c>EmailAddress()</c> and
/// <see cref="Pocs.Aspire.Domain.Users.ValueObjects.Email.From"/>'s <c>MailAddress.TryCreate</c>
/// check that <c>ValidEmail()</c> now delegates to, so the two can no longer disagree.
/// </summary>
public class ValidationExtensionsTests
{
    private sealed record EmailModel(string Email);

    private sealed class EmailModelValidator : AbstractValidator<EmailModel>
    {
        public EmailModelValidator() => RuleFor(x => x.Email).ValidEmail();
    }

    [Fact]
    public void ValidEmail_RejectsAddress_ThatEmailAddressValidatorUsedToAcceptButMailAddressRejects()
    {
        // Arrange
        var validator = new EmailModelValidator();
        var model = new EmailModel(".john@example.com");

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.ShouldBeFalse();
    }
}
