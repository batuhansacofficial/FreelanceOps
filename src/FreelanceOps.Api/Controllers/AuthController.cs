using FreelanceOps.Application.Identity.GetCurrentUser;
using FreelanceOps.Application.Identity.Login;
using FreelanceOps.Application.Identity.Logout;
using FreelanceOps.Application.Identity.RefreshToken;
using FreelanceOps.Application.Identity.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    RegisterHandler registerHandler,
    LoginHandler loginHandler,
    RefreshTokenHandler refreshTokenHandler,
    LogoutHandler logoutHandler,
    GetCurrentUserHandler getCurrentUserHandler) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<RegisterResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var response = await registerHandler.Handle(command, cancellationToken);

        return Created("/api/auth/me", response);
    }

    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var response = await loginHandler.Handle(command, cancellationToken);

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType<RefreshTokenResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var response = await refreshTokenHandler.Handle(command, cancellationToken);

        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        await logoutHandler.Handle(command, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var response = await getCurrentUserHandler.Handle(cancellationToken);

        return Ok(response);
    }
}
