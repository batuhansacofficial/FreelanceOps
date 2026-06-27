namespace FreelanceOps.Application.Abstractions.Authentication;

public interface ICurrentUserService
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }
}
