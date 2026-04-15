namespace SmartShip.Shared.Common.Exceptions;

/// <summary>
/// Domain model for request validation exception.
/// </summary>
public class RequestValidationException : Exception
{
    /// <summary>
    /// Processes request validation exception.
    /// </summary>
    public RequestValidationException() : base("The request failed validation.")
    {
    }

    /// <summary>
    /// Processes request validation exception.
    /// </summary>
    public RequestValidationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Processes request validation exception.
    /// </summary>
    public RequestValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
