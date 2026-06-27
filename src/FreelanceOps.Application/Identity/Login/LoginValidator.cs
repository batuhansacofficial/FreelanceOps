using FluentValidation;

namespace FreelanceOps.Application.Identity.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(command => command.Password)
            .NotEmpty();
    }
}
