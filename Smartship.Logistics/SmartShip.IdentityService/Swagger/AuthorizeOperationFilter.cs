/// <summary>
/// Provides backend implementation for AuthorizeOperationFilter.
/// </summary>

using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SmartShip.IdentityService.Swagger
{
    /// <summary>
    /// Represents AuthorizeOperationFilter.
    /// </summary>
    public class AuthorizeOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Executes Apply.
        /// </summary>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any()
                || (context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ?? false);

            if (hasAllowAnonymous)
            {
                return;
            }

            operation.Security ??= new List<OpenApiSecurityRequirement>();

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }
    }
}


