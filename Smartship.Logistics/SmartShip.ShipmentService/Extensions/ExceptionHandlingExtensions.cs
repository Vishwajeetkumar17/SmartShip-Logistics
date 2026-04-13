/// <summary>
/// Provides backend implementation for ExceptionHandlingExtensions.
/// </summary>

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartShip.Shared.Common.Exceptions;

namespace SmartShip.ShipmentService.Extensions;

/// <summary>
/// Represents ExceptionHandlingExtensions.
/// </summary>
public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Executes the UseGlobalExceptionHandling operation.
    /// </summary>
    public static void UseGlobalExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                var logger = context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GlobalExceptionHandling");

                if (exception is not null)
                {
                    logger.LogError(
                        exception,
                        "Unhandled exception for {Method} {Path}. TraceId: {TraceId}",
                        context.Request.Method,
                        context.Request.Path,
                        context.TraceIdentifier);
                }

                var (statusCode, title) = exception switch
                {
                    NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                    RequestValidationException => (StatusCodes.Status400BadRequest, "Request validation failed"),
                    ConflictException => (StatusCodes.Status409Conflict, "Resource conflict"),
                    UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                    _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = statusCode == StatusCodes.Status500InternalServerError
                        ? "An unexpected error occurred."
                        : exception?.Message,
                    Extensions =
                    {
                        ["traceId"] = context.TraceIdentifier
                    }
                });
            });
        });
    }
}


