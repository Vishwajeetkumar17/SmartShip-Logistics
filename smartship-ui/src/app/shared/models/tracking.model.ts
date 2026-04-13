/**
 * Tracking domain models for the Angular client.
 * Mirrors TrackingService DTOs for timeline/status/location interactions.
 */

// Matches backend: SmartShip.TrackingService DTOs

// Matches TrackingEventDTO from backend
export interface TrackingEvent {
  eventId?: number;
  trackingNumber: string;
  status: string;
  location: string;
  description: string;
  timestamp: string;
}

// Matches TrackingResponseDTO from backend
export interface TrackingResponse {
  trackingNumber: string;
  currentStatus: string;
  currentLocation: string;
  initialEventTimestamp?: string;
  latestEventTimestamp?: string;
  timeline: TrackingEvent[];
}

// Matches StatusUpdateDTO
export interface StatusUpdate {
  trackingNumber: string;
  status: string;
  location: string;
  description: string;
}

// Matches LocationUpdateDTO
export interface LocationUpdate {
  trackingNumber: string;
  latitude: number;
  longitude: number;
  description: string;
}
