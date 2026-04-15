namespace SmartShip.Shared.Common.Exceptions;

/// <summary>
/// Domain model for not found exception.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Processes not found exception.
    /// </summary>
    public NotFoundException() : base("The requested resource was not found.")
    {
    }

    /// <summary>
    /// Processes not found exception.
    /// </summary>
    public NotFoundException(string message) : base(message)
    {
    }

    /// <summary>
    /// Processes not found exception.
    /// </summary>
    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Processes not found exception.
    /// </summary>
    /// <param name="entityName">The name of the entity type (e.g., "Shipment", "Hub").</param>
    /// <param name="key">The identifier value that was not found.</param>
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with identifier '{key}' was not found.")
    {
    }
}
