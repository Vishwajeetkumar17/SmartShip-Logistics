namespace SmartShip.Shared.Common.Exceptions;

/// <summary>
/// Domain model for conflict exception.
/// </summary>
public class ConflictException : Exception
{
    /// <summary>
    /// Processes conflict exception.
    /// </summary>
    public ConflictException() : base("The request conflicts with the current state of the resource.")
    {
    }

    /// <summary>
    /// Processes conflict exception.
    /// </summary>
    public ConflictException(string message) : base(message)
    {
    }

    /// <summary>
    /// Processes conflict exception.
    /// </summary>
    public ConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Processes conflict exception.
    /// </summary>
    /// <param name="entityName">The name of the entity type (e.g., "User", "Hub").</param>
    /// <param name="conflictField">The field that caused the conflict (e.g., "Email", "Name").</param>
    /// <param name="conflictValue">The duplicate value.</param>
    public ConflictException(string entityName, string conflictField, object conflictValue)
        : base($"{entityName} with {conflictField} '{conflictValue}' already exists.")
    {
    }
}
