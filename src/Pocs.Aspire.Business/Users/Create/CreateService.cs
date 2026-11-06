using FluentValidation;
using LanguageExt;
using Pocs.Aspire.Business.Validations;
using Pocs.Aspire.Domain;
using Pocs.Aspire.Domain.Errors;
using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace Pocs.Aspire.Business.Users.Create;

internal class CreateService : ICreateService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateRequest> _validator;

    public CreateService(IUserRepository userRepository, IUnitOfWork unitOfWork, IValidator<CreateRequest> validator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public Task<Either<Failure, CreateResponse>> CreateAsync(CreateRequest request, CancellationToken cancellationToken = default) =>
        _validator.ValidateToEither(request, cancellationToken)
            .ToAsync()
            .Bind(valid => CreateValidatedAsync(valid, cancellationToken).ToAsync())
            .ToEither();

    private async Task<Either<Failure, CreateResponse>> CreateValidatedAsync(CreateRequest request, CancellationToken cancellationToken)
    {
        var email = Email.From(request.Email);
        var isEmailExists = await _userRepository.EmailExistsExceptForUser(email, null, cancellationToken);
        if (isEmailExists)
        {
            return new EmailAlreadyExistsError(email);
        }

        User user = request.ToDomain();

        await _userRepository.CreateAsync(user, cancellationToken);
        var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return saveResult.Map(_ => user.ToResponse());
    }
}
