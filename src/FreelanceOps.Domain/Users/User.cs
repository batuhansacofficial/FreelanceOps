namespace FreelanceOps.Domain.Users;

public sealed class User
{
    private readonly List<RefreshToken> _refreshTokens = [];

    private User()
    {
    }

    public User(string email, string passwordHash, string fullName)
    {
        Id = Guid.NewGuid();
        Email = NormalizeEmail(email);
        PasswordHash = passwordHash;
        FullName = fullName.Trim();
        CreatedAtUtc = DateTime.UtcNow;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    public static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    public void MarkLoggedIn()
    {
        LastLoginAtUtc = DateTime.UtcNow;
    }

    public RefreshToken AddRefreshToken(string tokenHash, DateTime expiresAtUtc)
    {
        var refreshToken = new RefreshToken(Id, tokenHash, expiresAtUtc);
        _refreshTokens.Add(refreshToken);

        return refreshToken;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
