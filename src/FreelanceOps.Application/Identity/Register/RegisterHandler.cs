using FluentValidation;
using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Identity.Register;

public sealed class RegisterHandler(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IValidator<RegisterCommand> validator)
{
    public async Task<RegisterResponse> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var email = User.NormalizeEmail(command.Email);
        var emailExists = await dbContext.Users
            .AnyAsync(user => user.Email == email, cancellationToken);

        if (emailExists)
        {
            throw new ConflictException("Email is already registered.");
        }

        var user = new User(email, passwordHasher.Hash(command.Password), command.FullName);

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(user.Id, user.Email, user.FullName);
    }
}
