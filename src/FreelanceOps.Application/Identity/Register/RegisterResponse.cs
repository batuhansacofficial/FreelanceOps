namespace FreelanceOps.Application.Identity.Register;

public sealed record RegisterResponse(Guid UserId, string Email, string FullName);
