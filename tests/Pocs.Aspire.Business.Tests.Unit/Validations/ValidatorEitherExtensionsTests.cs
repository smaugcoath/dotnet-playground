namespace Pocs.Aspire.Business.Tests.Unit.Validations;

using FluentValidation;
using LanguageExt;
using Pocs.Aspire.Business.Validations;
using Pocs.Aspire.Domain.Errors;
using Shouldly;
using System.Collections.Generic;
using System.Threading.Tasks;

internal sealed record Sample(string Name);

public class ValidatorEitherExtensionsTests
{
    private sealed class SampleValidator : AbstractValidator<Sample>
    {
        public SampleValidator() => RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
    }

    private readonly SampleValidator _validator = new();

    [Fact]
    public async Task ValidateToEither_ReturnsRequest_WhenValid()
    {
        // Arrange
        var request = new Sample("Ada");
        var expected = Either<Failure, Sample>.Right(request);

        // Act
        var actual = await _validator.ValidateToEither(request, TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task ValidateToEither_ReturnsValidationError_WhenInvalid()
    {
        // Arrange
        var request = new Sample("");
        var expected = Either<Failure, Sample>.Left(new ValidationError(new List<FieldError> { new("Name", "Name is required.") }));

        // Act
        var actual = await _validator.ValidateToEither(request, TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }
}
