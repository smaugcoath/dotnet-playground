namespace Pocs.Aspire.Business.Tests.Unit.Users.Update;

using Pocs.Aspire.Business.Users.Update;
using Shouldly;
using System;

/// <summary>
/// Covers the constructor's null-guard for <see cref="UpdateRequestValidator"/>.
/// This is not reachable through a functional test: the real app always resolves
/// <c>IUserRepository</c> via DI, so a null repository can only ever be exercised
/// by constructing the validator directly.
/// </summary>
public class UpdateRequestValidatorTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenUserRepositoryIsNull()
    {
        // Act
        var act = () => new UpdateRequestValidator(null!);

        // Assert
        var exception = Should.Throw<ArgumentNullException>(act);
        exception.ParamName.ShouldBe("userRepository");
    }
}
