using Pocs.Aspire.Domain.Users.ValueObjects;

namespace Pocs.Aspire.Domain.Users;

/// <summary>
/// Represents a user.
/// </summary>
/// <remarks>
/// This aggregate root includes identity and personal information for a user.
/// </remarks>
public class User
{
    /// <summary>
    /// The database name of the unique index enforcing email uniqueness.
    /// </summary>
    public const string EmailUniqueIndexName = "IX_User_Email";

    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public UserId Id { get; init; }

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    public FirstName FirstName { get; private set; }

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    public LastName LastName { get; private set; }

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public Email Email { get; private set; }

    private User(UserId id, FirstName firstName, LastName lastName, Email email) =>
            (Id, FirstName, LastName, Email) = (id, firstName, lastName, email);

    public static User From(UserId id, FirstName firstName, LastName lastName, Email email) =>
            new User(id, firstName, lastName, email);
    public static User New(FirstName firstName, LastName lastName, Email email) =>
        new User(UserId.New(), firstName, lastName, email);

    /// <summary>
    /// Changes the user's personal details.
    /// </summary>
    public void ChangeDetails(FirstName firstName, LastName lastName, Email email) =>
        (FirstName, LastName, Email) = (firstName, lastName, email);

}
