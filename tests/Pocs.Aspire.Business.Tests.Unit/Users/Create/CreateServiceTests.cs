namespace Pocs.Aspire.Business.Tests.Unit.Users.Create;

using LanguageExt;
using NSubstitute;
using Pocs.Aspire.Business.Users.Create;
using Pocs.Aspire.Domain;
using Pocs.Aspire.Domain.Errors;
using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Covers the mapping from a concurrent unique-constraint violation, raised by
/// <see cref="IUnitOfWork.SaveChangesAsync"/> under a race with another insert, to the
/// friendly <see cref="EmailAlreadyExistsError"/>. The pre-check in
/// <see cref="IUserRepository.EmailExistsExceptForUser"/> cannot reproduce this race in a
/// functional test, so it is covered here with NSubstitute.
/// </summary>
public class CreateServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateUserRequestValidator _validator;
    private readonly CreateService _sut;

    public CreateServiceTests()
    {
        _validator = new CreateUserRequestValidator(_userRepository);
        _sut = new CreateService(_userRepository, _unitOfWork, _validator);
    }

    [Fact]
    public async Task CreateAsync_ReturnsEmailAlreadyExistsError_WhenSaveChangesFailsOnEmailUniqueIndex()
    {
        // Arrange
        var request = new CreateRequest("Ada", "Lovelace", "ada.lovelace@example.com");
        _userRepository.EmailExistsExceptForUser(Arg.Any<Email>(), Arg.Any<UserId?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Either<Failure, Unit>.Left(new UniqueConstraintViolationError(User.EmailUniqueIndexName)));
        var expected = Either<Failure, CreateResponse>.Left(new EmailAlreadyExistsError(Email.From("ada.lovelace@example.com")));

        // Act
        var actual = await _sut.CreateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task CreateAsync_ReturnsUnderlyingFailure_WhenSaveChangesFailsOnAnotherConstraint()
    {
        // Arrange
        var request = new CreateRequest("Ada", "Lovelace", "ada.lovelace@example.com");
        _userRepository.EmailExistsExceptForUser(Arg.Any<Email>(), Arg.Any<UserId?>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(Either<Failure, Unit>.Left(new UniqueConstraintViolationError("IX_Other_Constraint")));
        var expected = Either<Failure, CreateResponse>.Left(new UniqueConstraintViolationError("IX_Other_Constraint"));

        // Act
        var actual = await _sut.CreateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldBeEquivalentTo(expected);
    }
}
