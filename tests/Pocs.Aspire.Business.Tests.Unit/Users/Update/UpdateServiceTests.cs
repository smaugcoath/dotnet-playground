namespace Pocs.Aspire.Business.Tests.Unit.Users.Update;

using LanguageExt;
using NSubstitute;
using Pocs.Aspire.Business.Users.Update;
using Pocs.Aspire.Domain;
using Pocs.Aspire.Domain.Errors;
using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;
using Shouldly;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Covers the mapping from a concurrent unique-constraint violation, raised by
/// <see cref="IUnitOfWork.SaveChangesAsync"/> under a race with another insert, to the
/// friendly <see cref="EmailAlreadyExistsError"/>. The pre-check in
/// <see cref="IUserRepository.EmailExistsExceptForUser"/> cannot reproduce this race in a
/// functional test, so it is covered here with NSubstitute.
/// </summary>
public class UpdateServiceTests
{
    private static readonly Guid ExistingUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateRequestValidator _validator;
    private readonly UpdateService _sut;

    public UpdateServiceTests()
    {
        _validator = new UpdateRequestValidator(_userRepository);
        _sut = new UpdateService(_userRepository, _unitOfWork, _validator);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsEmailAlreadyExistsError_WhenSaveChangesFailsOnEmailUniqueIndex()
    {
        // Arrange
        var request = new UpdateRequest(ExistingUserId, "Ada", "Lovelace", "ada.lovelace@example.com");
        var existingUser = User.From(
            UserId.From(ExistingUserId),
            FirstName.From("Ada"),
            LastName.From("Byron"),
            Email.From("ada.byron@example.com"));

        _userRepository.EmailExistsExceptForUser(Arg.Any<Email>(), Arg.Any<UserId?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _userRepository.GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(Option<User>.Some(existingUser));
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Either<Failure, Unit>.Left(new UniqueConstraintViolationError(User.EmailUniqueIndexName)));
        var expected = Either<Failure, UpdateResponse>.Left(new EmailAlreadyExistsError(Email.From("ada.lovelace@example.com")));

        // Act
        var actual = await _sut.UpdateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUnderlyingFailure_WhenSaveChangesFailsOnAnotherConstraint()
    {
        // Arrange
        var request = new UpdateRequest(ExistingUserId, "Ada", "Lovelace", "ada.lovelace@example.com");
        var existingUser = User.From(
            UserId.From(ExistingUserId),
            FirstName.From("Ada"),
            LastName.From("Byron"),
            Email.From("ada.byron@example.com"));

        _userRepository.EmailExistsExceptForUser(Arg.Any<Email>(), Arg.Any<UserId?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _userRepository.GetByIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(Option<User>.Some(existingUser));
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Either<Failure, Unit>.Left(new UniqueConstraintViolationError("IX_Other_Constraint")));
        var expected = Either<Failure, UpdateResponse>.Left(new UniqueConstraintViolationError("IX_Other_Constraint"));

        // Act
        var actual = await _sut.UpdateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }
}
