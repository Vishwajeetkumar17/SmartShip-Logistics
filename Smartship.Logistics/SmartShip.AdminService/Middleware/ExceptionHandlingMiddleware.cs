/// <summary>
/// Provides backend implementation for ExceptionHandlingMiddleware.
/// </summary>

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SmartShip.Shared.Common.Exceptions;
using SmartShip.Shared.Common.Services;

namespace SmartShip.AdminService.Middleware;

/// <summary>
/// Represents ExceptionHandlingMiddleware.
/// </summary>
public static class ExceptionHandlingMiddleware
{
    /// <summary>
    /// Executes UseGlobalExceptionHandling.
    /// </summary>
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

                var statusCode = exception switch
                {
                    NotFoundException => StatusCodes.Status404NotFound,
                    RequestValidationException => StatusCodes.Status400BadRequest,
                    UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                    HttpRequestException => StatusCodes.Status502BadGateway,
                    _ => StatusCodes.Status500InternalServerError
                };

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/problem+json";

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

    private static string GetTitle(Exception exception)
    {
        return exception switch
        {
            NotFoundException => "Resource Not Found",
            RequestValidationException => "Validation Error",
            UnauthorizedAccessException => "Forbidden",
            HttpRequestException => "Downstream Service Error",
            _ => "Server Error"
        };
    }
}


