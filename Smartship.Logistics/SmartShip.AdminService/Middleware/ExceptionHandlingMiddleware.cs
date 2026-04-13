/// <summary>
/// Global exception handling middleware for the Admin microservice.
/// Catches unhandled exceptions and returns standardised RFC 7807 ProblemDetails responses.
/// </summary>

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartShip.Shared.Common.Exceptions;
using SmartShip.Shared.Common.Services;

namespace SmartShip.AdminService.Middleware;

/// <summary>
/// Provides a global exception handling pipeline extension that maps
/// domain exceptions to appropriate HTTP status codes and structured error payloads.
/// </summary>
public static class ExceptionHandlingMiddleware
{
    #region Pipeline Extension

    /// <summary>
    /// Registers the global exception handler into the ASP.NET Core middleware pipeline.
    /// Maps known domain exceptions (NotFound, Validation, Conflict) to their HTTP equivalents
    /// and logs unhandled exceptions with correlation IDs for distributed tracing.
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
    /// Maps an exception type to a human-readable error title for the ProblemDetails response.
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


