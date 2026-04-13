/// <summary>
/// Custom exception thrown when a requested resource cannot be found.
/// Maps to HTTP 404 Not Found in exception handling middleware.
/// </summary>

namespace SmartShip.Shared.Common.Exceptions;

/// <summary>
/// Thrown when a requested entity or resource does not exist in the system.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the not found exception class.
    /// </summary>
    public NotFoundException() : base("The requested resource was not found.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the not found exception class.
    /// </summary>
    public NotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the not found exception class.
    /// </summary>
    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates a NotFoundException for a specific entity type and identifier.
    /// </summary>
    /// <param name="entityName">The name of the entity type (e.g., "Shipment", "Hub").</param>
    /// <param name="key">The identifier value that was not found.</param>
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with identifier '{key}' was not found.")
    {
    }
}
