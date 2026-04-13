export const TIMEZONE_CONFIG = {
  // Indian Standard Time timezone offset: UTC+5:30
  IST_OFFSET_HOURS: 5,
  IST_OFFSET_MINUTES: 30,
  IST_OFFSET_MS: (5.5 * 60 * 60 * 1000), // milliseconds
  
  // Timezone identifier
  TIMEZONE_NAME: 'Asia/Kolkata',
  TIMEZONE_ABBR: 'IST',
  
  // Format for displaying times
  DATE_FORMAT: 'mediumDate', // e.g., "Apr 3, 2026"
  DATETIME_FORMAT: 'medium', // e.g., "Apr 3, 2026, 2:30:00 PM"
  TIME_FORMAT: 'short', // e.g., "2:30 PM"
  
  /**
   * Converts UTC timestamp to IST
   * @param utcDate UTC date/time
   * @returns Date adjusted to IST
   */
  toIST(utcDate: Date | string): Date {
    const date = typeof utcDate === 'string' ? new Date(utcDate) : utcDate;
    return new Date(date.getTime() + this.IST_OFFSET_MS);
  },
  
  /**
   * Converts IST timestamp to UTC
   * @param istDate IST date/time
   * @returns Date adjusted to UTC
   */
  toUTC(istDate: Date | string): Date {
    const date = typeof istDate === 'string' ? new Date(istDate) : istDate;
    return new Date(date.getTime() - this.IST_OFFSET_MS);
  }
};
