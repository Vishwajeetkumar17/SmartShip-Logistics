/// <summary>
/// Custom exception thrown when a request conflicts with the current state of a resource.
/// Maps to HTTP 409 Conflict in exception handling middleware.
/// </summary>

namespace SmartShip.Shared.Common.Exceptions;

/// <summary>
/// Thrown when an operation cannot be completed because the resource already exists
/// or the action conflicts with the current state (e.g., duplicate entries, state transition violations).
/// </summary>
public class ConflictException : Exception
{
    /// <summary>
    /// Initializes a new instance of the conflict exception class.
    /// </summary>
    public ConflictException() : base("The request conflicts with the current state of the resource.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the conflict exception class.
    /// </summary>
    public ConflictException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the conflict exception class.
    /// </summary>
    public ConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Creates a ConflictException for a duplicate entity.
    /// </summary>
    /// <param name="entityName">The name of the entity type (e.g., "User", "Hub").</param>
    /// <param name="conflictField">The field that caused the conflict (e.g., "Email", "Name").</param>
    /// <param name="conflictValue">The duplicate value.</param>
    public ConflictException(string entityName, string conflictField, object conflictValue)
        : base($"{entityName} with {conflictField} '{conflictValue}' already exists.")
    {
    }
}
