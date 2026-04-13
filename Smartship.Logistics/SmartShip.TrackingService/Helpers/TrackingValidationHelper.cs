/// <summary>
/// Provides backend implementation for TrackingValidationHelper.
/// </summary>

using SmartShip.Shared.Common.Exceptions;
using SmartShip.TrackingService.DTOs;

namespace SmartShip.TrackingService.Helpers;

using SmartShip.Shared.Common.Exceptions;
using SmartShip.Shared.Common.Helpers;

/// <summary>
/// Represents TrackingValidationHelper.
/// </summary>
public static class TrackingValidationHelper
{
    /// <summary>
    /// Executes the NormalizeTrackingNumber operation.
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
    /// Executes the ValidateEvent operation.
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
    /// Executes the ValidateLocation operation.
    /// </summary>
    public static void ValidateLocation(LocationUpdateDTO dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        NormalizeTrackingNumber(dto.TrackingNumber);
        EnsureTimestampNotTooFarInFuture(dto.Timestamp, nameof(dto.Timestamp));
    }

    /// <summary>
    /// Executes the ValidateStatus operation.
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


