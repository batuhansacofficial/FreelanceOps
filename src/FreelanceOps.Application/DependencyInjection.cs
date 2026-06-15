using FluentValidation;
using FreelanceOps.Application.Clients.CreateClient;
using FreelanceOps.Application.Clients.DeleteClient;
using FreelanceOps.Application.Clients.GetClientById;
using FreelanceOps.Application.Clients.GetClients;
using FreelanceOps.Application.Clients.UpdateClient;
using FreelanceOps.Application.Identity.GetCurrentUser;
using FreelanceOps.Application.Identity.Login;
using FreelanceOps.Application.Identity.Logout;
using FreelanceOps.Application.Identity.RefreshToken;
using FreelanceOps.Application.Identity.Register;
using FreelanceOps.Application.Abstractions.Workspaces;
using FreelanceOps.Application.Workspaces;
using FreelanceOps.Application.Workspaces.CreateWorkspace;
using FreelanceOps.Application.Workspaces.DeleteWorkspace;
using FreelanceOps.Application.Workspaces.GetMyWorkspaces;
using FreelanceOps.Application.Workspaces.GetWorkspaceById;
using FreelanceOps.Application.Workspaces.Members.AddWorkspaceMember;
using FreelanceOps.Application.Workspaces.Members.ChangeWorkspaceMemberRole;
using FreelanceOps.Application.Workspaces.Members.GetWorkspaceMembers;
using FreelanceOps.Application.Workspaces.Members.RemoveWorkspaceMember;
using FreelanceOps.Application.Workspaces.RenameWorkspace;
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
        services.AddSingleton<ISlugGenerator, SlugGenerator>();
        services.AddScoped<IWorkspaceAuthorizationService, WorkspaceAuthorizationService>();
        services.AddScoped<CreateWorkspaceHandler>();
        services.AddScoped<GetMyWorkspacesHandler>();
        services.AddScoped<GetWorkspaceByIdHandler>();
        services.AddScoped<RenameWorkspaceHandler>();
        services.AddScoped<DeleteWorkspaceHandler>();
        services.AddScoped<GetWorkspaceMembersHandler>();
        services.AddScoped<AddWorkspaceMemberHandler>();
        services.AddScoped<ChangeWorkspaceMemberRoleHandler>();
        services.AddScoped<RemoveWorkspaceMemberHandler>();
        services.AddScoped<CreateClientHandler>();
        services.AddScoped<GetClientsHandler>();
        services.AddScoped<GetClientByIdHandler>();
        services.AddScoped<UpdateClientHandler>();
        services.AddScoped<DeleteClientHandler>();

        return services;
    }
}
