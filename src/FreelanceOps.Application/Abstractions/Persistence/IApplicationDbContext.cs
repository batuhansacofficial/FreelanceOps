using FreelanceOps.Domain.Users;
using FreelanceOps.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.Abstractions.Persistence;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<Workspace> Workspaces { get; }

    DbSet<WorkspaceMember> WorkspaceMembers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
