import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';

@Pipe({
  name: 'istDate',
  standalone: true
})
/**
 * Formats a given date value as IST (UTC+05:30) using Angular's DatePipe.
 * Accepts a Date, timestamp, or ISO string.
 */
export class IstDatePipe implements PipeTransform {
  private datePipe = new DatePipe('en-IN');
  private readonly IST_OFFSET = 5.5 * 60 * 60 * 1000; // IST is UTC+5:30

  transform(value: string | Date | number | null | undefined, format: string = 'mediumDate'): string | null {
    if (!value) {
      return null;
    }

    // Convert to Date if string
    let date: Date;
    if (typeof value === 'string') {
      date = new Date(value);
    } else if (typeof value === 'number') {
      date = new Date(value);
    } else {
      date = value;
    }

    // If the date is not valid, return null
    if (isNaN(date.getTime())) {
      return null;
    }

    // Convert UTC to IST
    const istDate = new Date(date.getTime() + this.IST_OFFSET);

    // Format using Angular's DatePipe with IST timezone
    return this.datePipe.transform(istDate, format, '+0530', 'en-IN') || null;
  }
}
