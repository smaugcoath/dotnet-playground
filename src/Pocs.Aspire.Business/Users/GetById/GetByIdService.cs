using FluentValidation;
using LanguageExt;
using Pocs.Aspire.Business.Validations;
using Pocs.Aspire.Domain;
using Pocs.Aspire.Domain.Errors;
using Pocs.Aspire.Domain.Users;
using Pocs.Aspire.Domain.Users.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace Pocs.Aspire.Business.Users.GetById;

internal class GetByIdService : IGetByIdService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<GetByIdRequest> _validator;

    public GetByIdService(IUserRepository userRepository, IValidator<GetByIdRequest> validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public Task<Either<Failure, GetByIdResponse>> GetByIdAsync(GetByIdRequest request, CancellationToken cancellationToken = default) =>
        _validator.ValidateToEither(request, cancellationToken)
            .ToAsync()
            .Bind(valid => GetByIdValidatedAsync(valid, cancellationToken).ToAsync())
            .ToEither();

    private async Task<Either<Failure, GetByIdResponse>> GetByIdValidatedAsync(GetByIdRequest request, CancellationToken cancellationToken)
    {
        var userId = UserId.From(request.Id);

        var userOption = await _userRepository.GetByIdAsync(userId, cancellationToken);

        return userOption
            .ToEither<Failure>(new NotFoundError(nameof(User)))
            .Bind<GetByIdResponse>(x => x.ToResponse());
    }


}
