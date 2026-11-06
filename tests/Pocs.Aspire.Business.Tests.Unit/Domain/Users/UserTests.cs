using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;
using Shouldly;

namespace Pocs.Aspire.Tests.Business.Tests.Unit.Domain.Users;

public class UserTests
{
    [Fact]
    public void ChangeDetails_ReplacesNameAndEmail_KeepingId()
    {
        // Arrange
        var id = UserId.From(System.Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var user = User.From(id, FirstName.From("John"), LastName.From("Doe"), Email.From("john@example.com"));
        var expected = User.From(id, FirstName.From("Jane"), LastName.From("Smith"), Email.From("jane@example.com"));

        // Act
        user.ChangeDetails(FirstName.From("Jane"), LastName.From("Smith"), Email.From("jane@example.com"));

        // Assert
        user.ShouldBeEquivalentTo(expected);
    }
}
