using FluentValidation;
using LanguageExt;
using Pocs.Aspire.Business.Validations;
using Pocs.Aspire.Domain.Errors;
using Pocs.Aspire.Domain.Users;
using System.Threading;
using System.Threading.Tasks;

namespace Pocs.Aspire.Business.Users.List;

internal class ListService : IListService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<ListRequest> _validator;

    public ListService(IUserRepository userRepository, IValidator<ListRequest> validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public Task<Either<Failure, ListResponse>> ListAsync(ListRequest request, CancellationToken cancellationToken = default) =>
        _validator.ValidateToEither(request, cancellationToken)
            .ToAsync()
            .Bind(valid => ListValidatedAsync(valid, cancellationToken).ToAsync())
            .ToEither();

    private async Task<Either<Failure, ListResponse>> ListValidatedAsync(ListRequest request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var users = await _userRepository.ListAsync(skip, request.PageSize, cancellationToken);
        var totalCount = await _userRepository.CountAsync(cancellationToken);

        return request.ToResponse(users, totalCount);
    }
}
