namespace Pocs.Aspire.Business.Tests.Unit.Users.Create;

using Pocs.Aspire.Business.Users.Create;
using Shouldly;
using System;

/// <summary>
/// Covers the constructor's null-guard for <see cref="CreateUserRequestValidator"/>.
/// This is not reachable through a functional test: the real app always resolves
/// <c>IUserRepository</c> via DI, so a null repository can only ever be exercised
/// by constructing the validator directly.
/// </summary>
public class CreateUserRequestValidatorTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenUserRepositoryIsNull()
    {
        // Act
        var act = () => new CreateUserRequestValidator(null!);

        // Assert
        var exception = Should.Throw<ArgumentNullException>(act);
        exception.ParamName.ShouldBe("userRepository");
    }
}
