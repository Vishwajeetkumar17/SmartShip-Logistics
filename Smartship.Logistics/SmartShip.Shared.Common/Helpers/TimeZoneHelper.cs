namespace SmartShip.Shared.Common.Helpers;

/// <summary>
/// Domain model for time zone helper.
/// </summary>
public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo IstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

    /// <summary>
    /// Returns current ist time.
    /// </summary>
    public static DateTime GetCurrentIstTime()
    {
        return TimeZoneInfo.ConvertTime(DateTime.UtcNow, IstTimeZone);
    }

    /// <summary>
    /// Returns current utc time.
    /// </summary>
    public static DateTime GetCurrentUtcTime()
    {
        return DateTime.UtcNow;
    }

    /// <summary>
    /// Converts utc to ist.
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
    /// Converts ist to utc.
    /// </summary>
    public static DateTime ConvertIstToUtc(DateTime istDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(istDateTime, IstTimeZone);
    }

    /// <summary>
    /// Returns ist offset.
    /// </summary>
    public static TimeSpan GetIstOffset()
    {
        return IstTimeZone.BaseUtcOffset;
    }
}


