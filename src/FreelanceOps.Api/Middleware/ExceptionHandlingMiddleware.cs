using FreelanceOps.Domain.Common;

namespace FreelanceOps.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException exception) when (!context.Response.HasStarted)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Domain rule violation",
                exception.Message);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            logger.LogError(exception, "Unhandled exception while processing request.");

            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Unexpected server error",
                "An unexpected error occurred.");
        }
    }

    private static Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        context.Response.StatusCode = statusCode;

        return Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail).ExecuteAsync(context);
    }
}
