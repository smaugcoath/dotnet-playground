using FluentValidation;
using Pocs.Aspire.Business.Validations;

namespace Pocs.Aspire.Business.Users.Delete;
public class DeleteRequestValidator : AbstractValidator<DeleteRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRequestValidator"/> class.
    /// </summary>
    public DeleteRequestValidator()
    {
        RuleFor(x => x.Id).ValidUserId();
    }
}
