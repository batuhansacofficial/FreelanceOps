using FreelanceOps.Application.Abstractions.Persistence;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace FreelanceOps.Application.TimeTracking;

internal static class TimeTrackingTaskGuard
{
    public static async Task<ProjectTask> GetActiveTaskAsync(
        IApplicationDbContext dbContext,
        Guid workspaceId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var task = await dbContext.ProjectTasks
            .FirstOrDefaultAsync(
                task =>
                    task.Id == taskId &&
                    task.WorkspaceId == workspaceId &&
                    !task.IsDeleted,
                cancellationToken);

        if (task is null)
        {
            throw new NotFoundException("Project task was not found.");
        }

        var projectExists = await dbContext.Projects
            .AnyAsync(
                project =>
                    project.Id == task.ProjectId &&
                    project.WorkspaceId == workspaceId &&
                    !project.IsDeleted,
                cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException("Project task was not found.");
        }

        return task;
    }
}
