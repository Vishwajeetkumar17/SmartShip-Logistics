namespace SmartShip.Shared.Common.Helpers;

/// <summary>
/// Helper class to handle Indian Standard Time (IST) conversions across the application.
/// IST is UTC+5:30
/// </summary>
public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo IstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

    /// <summary>
    /// Gets the current time in Indian Standard Time (IST)
    /// </summary>
    public static DateTime GetCurrentIstTime()
    {
        return TimeZoneInfo.ConvertTime(DateTime.UtcNow, IstTimeZone);
    }

    /// <summary>
    /// Gets the current UTC time (for database storage)
    /// </summary>
    public static DateTime GetCurrentUtcTime()
    {
        return DateTime.UtcNow;
    }

    /// <summary>
    /// Converts a UTC DateTime to IST DateTime
    /// </summary>
    public static DateTime ConvertUtcToIst(DateTime utcDateTime)
    {
        if (utcDateTime.Kind != DateTimeKind.Utc)
        {
            utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }
        return TimeZoneInfo.ConvertTime(utcDateTime, IstTimeZone);
    }

    /// <summary>
    /// Converts an IST DateTime to UTC DateTime
    /// </summary>
    public static DateTime ConvertIstToUtc(DateTime istDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(istDateTime, IstTimeZone);
    }

    /// <summary>
    /// Gets the IST offset from UTC (5:30)
    /// </summary>
    public static TimeSpan GetIstOffset()
    {
        return IstTimeZone.BaseUtcOffset;
    }
}


