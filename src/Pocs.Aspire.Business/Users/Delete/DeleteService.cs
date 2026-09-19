using FluentValidation;
using LanguageExt;
using Pocs.Aspire.Business.Validations;
using Pocs.Aspire.Domain;
using Pocs.Aspire.Domain.Errors;
using Pocs.Aspire.Domain.Users;
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

    public async Task<Either<Failure, Unit>> DeleteAsync(DeleteRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToFieldErrors());
        }

        var userId = request.ToDomain();

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
