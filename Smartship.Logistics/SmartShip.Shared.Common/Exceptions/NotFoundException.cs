/// <summary>
/// Provides backend implementation for NotFoundException.
/// </summary>

namespace SmartShip.Shared.Common.Exceptions;

/// <summary>
/// Represents NotFoundException.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}


