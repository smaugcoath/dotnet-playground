using FluentValidation;
using Pocs.Aspire.Business.Validations;

namespace Pocs.Aspire.Business.Users.GetById;
public class GetByIdRequestValidator : AbstractValidator<GetByIdRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetByIdRequestValidator"/> class.
    /// </summary>
    public GetByIdRequestValidator()
    {
        RuleFor(u => u.Id).ValidUserId();
    }
}
