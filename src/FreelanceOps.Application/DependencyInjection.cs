using FluentValidation;
using FreelanceOps.Application.Identity.GetCurrentUser;
using FreelanceOps.Application.Identity.Login;
using FreelanceOps.Application.Identity.Logout;
using FreelanceOps.Application.Identity.RefreshToken;
using FreelanceOps.Application.Identity.Register;
using Microsoft.Extensions.DependencyInjection;

namespace FreelanceOps.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<GetCurrentUserHandler>();

        return services;
    }
}
