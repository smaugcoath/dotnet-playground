using LanguageExt;
using Pocs.Aspire.Domain.Users.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pocs.Aspire.Domain.Errors;

public abstract record Failure
{
    public string Code { get; private init; }
    public string Message { get; private init; }

    protected Failure(string code, string message) => (Code, Message) = (code, message);
}


public sealed record NotFoundError(string Entity) : Failure("ERR-001", $"Entity '{Entity}' was not found.");

public record FieldError(string Field, string Message);
public sealed record ValidationError(IEnumerable<FieldError> Errors) : Failure("ERR-002", $"{Errors.Count()} validation errors occurred.");

public sealed record EmailAlreadyExistsError(Email Email) : Failure("ERR-003", $"The email {Email} already exists.");

/// <summary>
/// Represents a write rejected because it would duplicate a value that must be unique,
/// when no more specific domain failure applies. Deliberately carries no storage detail.
/// </summary>
public sealed record UniqueConstraintViolationError() : Failure("ERR-004", "A unique value was already taken.");
