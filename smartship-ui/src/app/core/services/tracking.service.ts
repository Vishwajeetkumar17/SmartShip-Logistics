import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TrackingResponse, TrackingEvent, StatusUpdate, LocationUpdate } from '../../shared/models/tracking.model';

@Injectable({
  providedIn: 'root'
})
/**
 * Tracking API client.
 * Customer endpoints read tracking info by tracking number; admin endpoints manage tracking events and status updates.
 */
export class TrackingService {
  private readonly apiUrl = `${environment.apiBaseUrl}/tracking/api/tracking`;

  constructor(private http: HttpClient) {}

  // Get full tracking info (includes timeline)
  getTrackingInfo(trackingNumber: string): Observable<TrackingResponse> {
    return this.http.get<TrackingResponse>(`${this.apiUrl}/${trackingNumber}`);
  }

  // Get timeline events
  getTimeline(trackingNumber: string): Observable<TrackingEvent[]> {
    return this.http.get<TrackingEvent[]>(`${this.apiUrl}/${trackingNumber}/timeline`);
  }

  // Get events
  getEvents(trackingNumber: string): Observable<TrackingEvent[]> {
    return this.http.get<TrackingEvent[]>(`${this.apiUrl}/${trackingNumber}/events`);
  }

  // Get current status
  getStatus(trackingNumber: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${trackingNumber}/status`);
  }

  // Get latest location
  getLatestLocation(trackingNumber: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/location/${trackingNumber}`);
  }

  // === Admin Methods ===

  // Add tracking event
  addEvent(event: TrackingEvent): Observable<TrackingEvent> {
    return this.http.post<TrackingEvent>(`${this.apiUrl}/events`, event);
  }

  // Update tracking event
  updateEvent(id: number, event: TrackingEvent): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/events/${id}`, event);
  }

  // Delete tracking event
  deleteEvent(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/events/${id}`);
  }

  // Add location update
  addLocationUpdate(location: LocationUpdate): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/location`, location);
  }

  // Update delivery status
  updateDeliveryStatus(trackingNumber: string, status: StatusUpdate): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${trackingNumber}/status`, status);
  }
}
