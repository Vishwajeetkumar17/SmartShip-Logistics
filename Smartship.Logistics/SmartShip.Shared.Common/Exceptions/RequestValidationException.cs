/// <summary>
/// Provides backend implementation for RequestValidationException.
/// </summary>

namespace SmartShip.Shared.Common.Exceptions;

/// <summary>
/// Represents RequestValidationException.
/// </summary>
public class RequestValidationException : Exception
{
    public RequestValidationException(string message) : base(message)
    {
    }
}


