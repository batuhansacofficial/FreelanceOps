using FluentValidation;
using FreelanceOps.Application.Common.Exceptions;
using FreelanceOps.Domain.Common;
using Microsoft.AspNetCore.Mvc;

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
        catch (ValidationException exception) when (!context.Response.HasStarted)
        {
            var errors = exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await Results.ValidationProblem(
                errors,
                title: "Validation failed",
                statusCode: StatusCodes.Status400BadRequest).ExecuteAsync(context);
        }
        catch (ConflictException exception) when (!context.Response.HasStarted)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message);
        }
        catch (UnauthorizedException exception) when (!context.Response.HasStarted)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                exception.Message);
        }
        catch (ForbiddenException exception) when (!context.Response.HasStarted)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Forbidden",
                exception.Message);
        }
        catch (NotFoundException exception) when (!context.Response.HasStarted)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status404NotFound,
                "Not found",
                exception.Message);
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
