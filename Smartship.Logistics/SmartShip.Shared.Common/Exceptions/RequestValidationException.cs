/// <summary>
/// Custom exception thrown when a client request fails validation.
/// Maps to HTTP 400 Bad Request in exception handling middleware.
/// </summary>

namespace SmartShip.Shared.Common.Exceptions;

/// <summary>
/// Thrown when input data or request parameters fail business validation rules.
/// </summary>
public class RequestValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the request validation exception class.
    /// </summary>
    public RequestValidationException() : base("The request failed validation.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the request validation exception class.
    /// </summary>
    public RequestValidationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the request validation exception class.
    /// </summary>
    public RequestValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
