using FluentValidation;
using Pocs.Aspire.Business.Validations;

namespace Pocs.Aspire.Business.Users.List;
public class ListRequestValidator : AbstractValidator<ListRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListRequestValidator"/> class.
    /// </summary>
    public ListRequestValidator()
    {
        RuleFor(x => x.Page).ValidPage();
        RuleFor(x => x.PageSize).ValidPageSize();
    }
}
