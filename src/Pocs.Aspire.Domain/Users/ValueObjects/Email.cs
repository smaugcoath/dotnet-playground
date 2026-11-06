using LanguageExt;
using Pocs.Aspire.Domain.Errors;
using System;
using System.Net.Mail;

namespace Pocs.Aspire.Domain.Users.ValueObjects;

/// <summary>
/// Represents an email address in the user domain.
/// </summary>
/// <remarks>
/// An email is structurally valid and never longer than <see cref="MaxLength"/> characters.
/// </remarks>
public readonly record struct Email
{
    public const int MaxLength = 100;
    public const string InvalidMessage = "A valid email is required.";
    public static readonly string TooLongMessage = $"Emails cannot exceed {MaxLength} characters.";

    public string Value { get; init; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates an email, or a <see cref="ValidationError"/> when the value breaks an invariant.
    /// </summary>
    public static Either<Failure, Email> Create(string value) =>
        Validate(value) is { } message
            ? new ValidationError([new FieldError(nameof(Email), message)])
            : new Email(value);

    /// <summary>
    /// Creates an email from a value already known to be valid; throws when it is not.
    /// </summary>
    public static Email From(string value) =>
        Validate(value) is { } message
            ? throw new ArgumentException(message, nameof(value))
            : new Email(value);

    /// <summary>
    /// Determines whether the given value is a structurally valid email address.
    /// </summary>
    public static bool IsValid(string? value) => MailAddress.TryCreate(value, out _);

    public static bool IsWithinMaxLength(string? value) => value is null || value.Length <= MaxLength;

    private static string? Validate(string? value)
    {
        if (!IsValid(value))
        {
            return InvalidMessage;
        }

        return IsWithinMaxLength(value) ? null : TooLongMessage;
    }

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();

    public static implicit operator string(Email value) => value.Value;
}
