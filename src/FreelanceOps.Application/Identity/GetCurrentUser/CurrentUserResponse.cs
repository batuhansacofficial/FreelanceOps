namespace FreelanceOps.Application.Identity.GetCurrentUser;

public sealed record CurrentUserResponse(Guid Id, string Email, string FullName);
