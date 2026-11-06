using FluentValidation;
using LanguageExt;
using Pocs.Aspire.Business.Validations;
using Pocs.Aspire.Domain;
using Pocs.Aspire.Domain.Errors;
using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace Pocs.Aspire.Business.Users.Delete;

internal class DeleteService : IDeleteService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<DeleteRequest> _validator;

    public DeleteService(IUserRepository userRepository, IUnitOfWork unitOfWork, IValidator<DeleteRequest> validator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public Task<Either<Failure, Unit>> DeleteAsync(DeleteRequest request, CancellationToken cancellationToken = default) =>
        _validator.ValidateToEither(request, cancellationToken)
            .ToAsync()
            .Bind(valid => DeleteValidatedAsync(valid, cancellationToken).ToAsync())
            .ToEither();

    private async Task<Either<Failure, Unit>> DeleteValidatedAsync(DeleteRequest request, CancellationToken cancellationToken)
    {
        var userId = UserId.From(request.Id);

        var userOption = await _userRepository.GetByIdAsync(userId, cancellationToken);

        return await userOption
            .ToEither<Failure>(new NotFoundError(nameof(User)))
            .ToAsync()
            .BindAsync<Unit>
            (
                async user =>
                {
                    await _userRepository.DeleteAsync(user, cancellationToken);
                    var saveResult = await _unitOfWork.SaveChangesAsync(cancellationToken);

                    return saveResult.ToAsync();
                }
            ).ToEither();
    }
}
