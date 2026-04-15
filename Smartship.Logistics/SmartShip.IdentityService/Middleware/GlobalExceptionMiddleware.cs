using Microsoft.AspNetCore.Mvc;
using SmartShip.Shared.Common.Exceptions;
using SmartShip.Shared.Common.Services;

namespace SmartShip.IdentityService.Middleware
{
    /// <summary>
    /// Catches unhandled exceptions and returns consistent problem-details style JSON responses.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        /// <summary>
        /// Creates middleware that logs and translates exceptions for the identity API.
        /// </summary>
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the next delegate; on failure writes a JSON error body with appropriate status.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}. TraceId: {TraceId}",
                    context.Request.Method, context.Request.Path, context.TraceIdentifier);
                await WriteProblemAsync(context, ex);
            }
        }

        private static async Task WriteProblemAsync(HttpContext context, Exception ex)
        {
            var (status, title) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
                RequestValidationException => (StatusCodes.Status400BadRequest, "Validation Error"),
                ConflictException => (StatusCodes.Status409Conflict, "Resource Conflict"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                _ => (StatusCodes.Status500InternalServerError, "Server Error")
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            var correlationId = context.RequestServices
                .GetService<ICorrelationIdService>()
                ?.GetCorrelationId();

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred."
                    : ex.Message,
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                problem.Extensions["correlationId"] = correlationId;
            }

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
