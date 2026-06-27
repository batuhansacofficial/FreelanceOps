using FreelanceOps.Application.Abstractions.Authentication;
using FreelanceOps.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace FreelanceOps.Infrastructure.Authentication;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _passwordHasher = new();

    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(default!, password);
    }

    public bool Verify(string passwordHash, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(default!, passwordHash, password);

        return result != PasswordVerificationResult.Failed;
    }
}
