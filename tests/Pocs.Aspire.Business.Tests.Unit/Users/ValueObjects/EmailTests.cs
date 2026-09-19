namespace Pocs.Aspire.Business.Tests.Unit.Users.ValueObjects;

using Pocs.Aspire.Domain.Users.ValueObjects;
using Shouldly;

public class EmailTests
{
    [Fact]
    public void IsValid_ReturnsTrue_WhenValueIsAStructurallyValidEmail()
    {
        // Act
        var actual = Email.IsValid("ada.lovelace@example.com");

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenValueIsNotAStructurallyValidEmail()
    {
        // Act
        var actual = Email.IsValid("not-an-email");

        // Assert
        actual.ShouldBeFalse();
    }
}
