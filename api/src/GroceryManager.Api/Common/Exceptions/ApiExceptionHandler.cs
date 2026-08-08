using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroceryManager.Api.Common.Exceptions;

public sealed partial class ApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ValidationException or ArgumentException =>
                (StatusCodes.Status400BadRequest, "Invalid request"),
            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Authentication required"),
            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException or DbUpdateConcurrencyException =>
                (StatusCodes.Status409Conflict, "Request conflict"),
            _ =>
                (StatusCodes.Status500InternalServerError, "Unexpected server error")
        };

        if (status == StatusCodes.Status500InternalServerError)
            LogUnhandledException(logger, exception, httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred."
                    : exception.Message,
                Instance = httpContext.Request.Path
            }
        });
    }

    [LoggerMessage(LogLevel.Error, "Unhandled exception for {Method} {Path}")]
    private static partial void LogUnhandledException(
        ILogger logger,
        Exception exception,
        string method,
        PathString path);
}
