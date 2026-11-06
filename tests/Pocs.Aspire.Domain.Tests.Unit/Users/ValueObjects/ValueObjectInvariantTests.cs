using LanguageExt;
using Pocs.Aspire.Domain.Errors;
using Pocs.Aspire.Domain.Users.ValueObjects;
using Shouldly;
using System;

namespace Pocs.Aspire.Domain.Tests.Unit.Users.ValueObjects;

public class ValueObjectInvariantTests
{
    private static readonly string Name50 = new('a', 50);
    private static readonly string Name51 = new('a', 51);
    private static readonly string Email100 = new string('a', 88) + "@example.com";
    private static readonly string Email101 = new string('a', 89) + "@example.com";

    [Fact]
    public void FirstName_Create_AtMaxLength_ReturnsRight()
    {
        // Act
        var actual = FirstName.Create(Name50);

        // Assert
        actual.ShouldBeEquivalentTo(Either<Failure, FirstName>.Right(FirstName.From(Name50)));
    }

    [Theory]
    [InlineData("", "First name is required.")]
    [InlineData("   ", "First name is required.")]
    [InlineData(null, "First name is required.")]
    public void FirstName_Create_Blank_ReturnsValidationError(string? value, string message)
    {
        // Act
        var actual = FirstName.Create(value!);

        // Assert
        AssertSingleFieldError(actual, nameof(FirstName), message);
    }

    [Fact]
    public void FirstName_Create_TooLong_ReturnsValidationError()
    {
        // Act
        var actual = FirstName.Create(Name51);

        // Assert
        AssertSingleFieldError(actual, nameof(FirstName), "First name cannot exceed 50 characters.");
    }

    [Fact]
    public void FirstName_From_TooLong_Throws()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => FirstName.From(Name51));
    }

    [Fact]
    public void LastName_Create_AtMaxLength_ReturnsRight()
    {
        // Act
        var actual = LastName.Create(Name50);

        // Assert
        actual.ShouldBeEquivalentTo(Either<Failure, LastName>.Right(LastName.From(Name50)));
    }

    [Theory]
    [InlineData("", "Last name is required.")]
    [InlineData("   ", "Last name is required.")]
    public void LastName_Create_Blank_ReturnsValidationError(string value, string message)
    {
        // Act
        var actual = LastName.Create(value);

        // Assert
        AssertSingleFieldError(actual, nameof(LastName), message);
    }

    [Fact]
    public void LastName_Create_TooLong_ReturnsValidationError()
    {
        // Act
        var actual = LastName.Create(Name51);

        // Assert
        AssertSingleFieldError(actual, nameof(LastName), "Last name cannot exceed 50 characters.");
    }

    [Fact]
    public void LastName_From_TooLong_Throws()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => LastName.From(Name51));
    }

    [Fact]
    public void Email_Create_AtMaxLength_ReturnsRight()
    {
        // Act
        var actual = Email.Create(Email100);

        // Assert
        actual.ShouldBeEquivalentTo(Either<Failure, Email>.Right(Email.From(Email100)));
    }

    [Fact]
    public void Email_Create_Malformed_ReturnsValidationError()
    {
        // Act
        var actual = Email.Create("not-an-email");

        // Assert
        AssertSingleFieldError(actual, nameof(Email), "A valid email is required.");
    }

    [Fact]
    public void Email_Create_TooLong_ReturnsValidationError()
    {
        // Act
        var actual = Email.Create(Email101);

        // Assert
        AssertSingleFieldError(actual, nameof(Email), "Emails cannot exceed 100 characters.");
    }

    [Fact]
    public void Email_From_TooLong_Throws()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => Email.From(Email101));
    }

    private static void AssertSingleFieldError<T>(Either<Failure, T> actual, string field, string message)
    {
        var failure = actual.Match(Right: _ => null!, Left: f => f);
        var error = failure.ShouldBeOfType<ValidationError>();
        error.Errors.ShouldBe([new FieldError(field, message)]);
    }
}
