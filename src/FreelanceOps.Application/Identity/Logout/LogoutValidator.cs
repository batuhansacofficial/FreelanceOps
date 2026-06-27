using FluentValidation;

namespace FreelanceOps.Application.Identity.Logout;

public sealed class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty();
    }
}
