using FluentValidation;
using LanguageExt;
using Pocs.Aspire.Business.Validations;
using Pocs.Aspire.Domain;
using Pocs.Aspire.Domain.Errors;
using Pocs.Aspire.Domain.Users;
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

    public async Task<Either<Failure, GetByIdResponse>> GetByIdAsync(GetByIdRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return new ValidationError(validationResult.ToFieldErrors());
        }

        var userId = request.ToDomain();

        var userOption = await _userRepository.GetByIdAsync(userId, cancellationToken);

        return userOption
            .ToEither<Failure>(new NotFoundError(nameof(User)))
            .Bind<GetByIdResponse>(x => x.ToResponse());
    }


}
