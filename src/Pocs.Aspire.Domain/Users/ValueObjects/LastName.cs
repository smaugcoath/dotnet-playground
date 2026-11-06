using LanguageExt;
using Pocs.Aspire.Domain.Errors;
using System;

namespace Pocs.Aspire.Domain.Users.ValueObjects;

/// <summary>
/// Represents a strongly-typed last name for a <see cref="User"/> entity.
/// </summary>
/// <remarks>
/// A last name is never blank and never longer than <see cref="MaxLength"/> characters.
/// </remarks>
public readonly record struct LastName
{
    public const int MaxLength = 50;
    public const string RequiredMessage = "Last name is required.";
    public static readonly string TooLongMessage = $"Last name cannot exceed {MaxLength} characters.";

    public string Value { get; init; }

    private LastName(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a last name, or a <see cref="ValidationError"/> when the value breaks an invariant.
    /// </summary>
    public static Either<Failure, LastName> Create(string value) =>
        Validate(value) is { } message
            ? new ValidationError([new FieldError(nameof(LastName), message)])
            : new LastName(value);

    /// <summary>
    /// Creates a last name from a value already known to be valid; throws when it is not.
    /// </summary>
    public static LastName From(string value) =>
        Validate(value) is { } message
            ? throw new ArgumentException(message, nameof(value))
            : new LastName(value);

    public static bool IsNotBlank(string? value) => !string.IsNullOrWhiteSpace(value);

    public static bool IsWithinMaxLength(string? value) => value is null || value.Length <= MaxLength;

    private static string? Validate(string? value)
    {
        if (!IsNotBlank(value))
        {
            return RequiredMessage;
        }

        return IsWithinMaxLength(value) ? null : TooLongMessage;
    }

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();

    public static implicit operator string(LastName value) => value.Value;
}
