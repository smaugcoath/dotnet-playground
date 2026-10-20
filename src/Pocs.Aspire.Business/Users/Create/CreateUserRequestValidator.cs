using FluentValidation;
using Pocs.Aspire.Business.Validations;
using Pocs.Aspire.Domain.Users;
using System;

namespace Pocs.Aspire.Business.Users.Create;
public class CreateUserRequestValidator : AbstractValidator<CreateRequest>
{
    public CreateUserRequestValidator(IUserRepository userRepository)
    {
        ArgumentNullException.ThrowIfNull(userRepository);

        RuleFor(u => u.FirstName).ValidFirstName();
        RuleFor(u => u.LastName).ValidLastName();
        RuleFor(u => u.Email).ValidEmail();
    }
}
