using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartShip.Shared.Common.Exceptions;
using SmartShip.Shared.Common.Services;

namespace SmartShip.AdminService.Middleware;

/// <summary>
/// Middleware component for exception handling request pipeline behavior.
/// </summary>
public static class ExceptionHandlingMiddleware
{
    #region Pipeline Extension

    /// <summary>
    /// Registers centralized exception-to-response handling for admin APIs.
    /// </summary>
    /// <param name="app">The application builder instance.</param>
    public static void UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature == null) return;

                var exception = contextFeature.Error;
                var logger = context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GlobalExceptionHandling");
                logger.LogError(
                    exception,
                    "Unhandled exception for {Method} {Path}. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.TraceIdentifier);

                // Map domain exceptions to HTTP status codes
                var statusCode = exception switch
                {
                    NotFoundException => StatusCodes.Status404NotFound,
                    RequestValidationException => StatusCodes.Status400BadRequest,
                    ConflictException => StatusCodes.Status409Conflict,
                    UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                    HttpRequestException => StatusCodes.Status502BadGateway,
                    _ => StatusCodes.Status500InternalServerError
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";

                // Attach correlation ID for distributed tracing
                var correlationId = context.RequestServices
                    .GetRequiredService<ICorrelationIdService>()
                    .GetCorrelationId();

                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = statusCode,
                    Title = GetTitle(exception),
                    Detail = statusCode == StatusCodes.Status500InternalServerError
                        ? "An unexpected error occurred."
                        : exception.Message,
                    Extensions =
                    {
                        ["traceId"] = context.TraceIdentifier,
                        ["correlationId"] = correlationId
                    }
                });
            });
        });
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Returns title.
    /// </summary>
    /// <param name="exception">The caught exception.</param>
    /// <returns>A short, descriptive error title string.</returns>
    private static string GetTitle(Exception exception)
    {
        return exception switch
        {
            NotFoundException => "Resource Not Found",
            RequestValidationException => "Validation Error",
            ConflictException => "Resource Conflict",
            UnauthorizedAccessException => "Forbidden",
            HttpRequestException => "Downstream Service Error",
            _ => "Server Error"
        };
    }

    #endregion
}


