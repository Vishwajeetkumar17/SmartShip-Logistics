using SmartShip.TrackingService.DTOs;
using SmartShip.Shared.Common.Exceptions;
using SmartShip.Shared.Common.Helpers;

namespace SmartShip.TrackingService.Helpers;

/// <summary>
/// Domain model for tracking validation helper.
/// </summary>
public static class TrackingValidationHelper
{
    /// <summary>
    /// Normalizes tracking number.
    /// </summary>
    public static string NormalizeTrackingNumber(string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new RequestValidationException("Tracking number is required.");
        }

        return trackingNumber.Trim().ToUpperInvariant();
    }

    /// <summary>
    /// Validates event.
    /// </summary>
    public static void ValidateEvent(TrackingEventDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        NormalizeTrackingNumber(dto.TrackingNumber);
        EnsureRequiredValue(dto.Status, "Status");
        EnsureRequiredValue(dto.Location, "Location");
        EnsureTimestampNotTooFarInFuture(dto.Timestamp, nameof(dto.Timestamp));
    }

    /// <summary>
    /// Validates location.
    /// </summary>
    public static void ValidateLocation(LocationUpdateDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        NormalizeTrackingNumber(dto.TrackingNumber);
        EnsureTimestampNotTooFarInFuture(dto.Timestamp, nameof(dto.Timestamp));
    }

    /// <summary>
    /// Validates status.
    /// </summary>
    public static void ValidateStatus(StatusUpdateDTO dto, string trackingNumber)
    {
        ArgumentNullException.ThrowIfNull(dto);

        NormalizeTrackingNumber(trackingNumber);
        EnsureRequiredValue(dto.Status, "Status");
        EnsureTimestampNotTooFarInFuture(TimeZoneHelper.GetCurrentUtcTime(), "Timestamp");
    }

    private static void EnsureRequiredValue(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new RequestValidationException($"{fieldName} is required.");
        }
    }

    private static void EnsureTimestampNotTooFarInFuture(DateTime timestamp, string fieldName)
    {
        if (timestamp != default && timestamp > TimeZoneHelper.GetCurrentUtcTime().AddMinutes(5))
        {
            throw new RequestValidationException($"{fieldName} cannot be more than 5 minutes in the future.");
        }
    }
}


