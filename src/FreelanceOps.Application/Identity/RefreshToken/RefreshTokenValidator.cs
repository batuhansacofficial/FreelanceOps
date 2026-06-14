using FluentValidation;

namespace FreelanceOps.Application.Identity.RefreshToken;

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty();
    }
}
