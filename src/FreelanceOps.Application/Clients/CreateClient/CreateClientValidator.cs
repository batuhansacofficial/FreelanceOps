using FluentValidation;

namespace FreelanceOps.Application.Clients.CreateClient;

public sealed class CreateClientValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientValidator()
    {
        RuleFor(command => command.WorkspaceId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(command => command.Email)
            .MaximumLength(320)
            .EmailAddress()
            .When(command => !string.IsNullOrWhiteSpace(command.Email));

        RuleFor(command => command.CompanyName)
            .MaximumLength(160);

        RuleFor(command => command.Notes)
            .MaximumLength(2000);
    }
}
