/// <summary>
/// Provides backend implementation for GlobalExceptionMiddleware.
/// </summary>

using Microsoft.AspNetCore.Mvc;

namespace SmartShip.IdentityService.Middleware
{
    /// <summary>
    /// Represents GlobalExceptionMiddleware.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Executes InvokeAsync.
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteProblemAsync(context, ex);
            }
        }

        private static async Task WriteProblemAsync(HttpContext context, Exception ex)
        {
            var (status, title) = ex switch
            {
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                InvalidOperationException invalidOp => (
                    StatusCodes.Status400BadRequest,
                    string.IsNullOrWhiteSpace(invalidOp.Message) ? "Request could not be processed" : invalidOp.Message
                ),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                _ => (StatusCodes.Status500InternalServerError, "Unexpected server error")
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred."
                    : null,
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}


