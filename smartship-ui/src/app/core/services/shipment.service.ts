import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, map, of, switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ShipmentResponse, CreateShipmentRequest, UpdateShipmentRequest,
  RateRequest, RateResponse, ShippingService, Package
} from '../../shared/models/shipment.model';
import { PaginatedResponse } from '../../shared/models/pagination.model';

export interface BookShipmentRequest {
  hubName: string;
  hubAddress?: string;
}

export interface ShipmentStatusUpdateRequest {
  hubLocation?: string;
}

export interface RaiseShipmentIssueRequest {
  issueType: string;
  description: string;
}

@Injectable({
  providedIn: 'root'
})
/**
 * Shipment API client used by both customer and admin screens.
 * Includes helpers to aggregate paginated endpoints into a single list when needed.
 */
export class ShipmentService {
  private readonly apiUrl = `${environment.apiBaseUrl}/shipment/api/shipments`;

  constructor(private http: HttpClient) {}

  // Customer: get my shipments
  getMyShipments(): Observable<ShipmentResponse[]> {
    return this.getMyShipmentsPage(1, 5).pipe(map((response) => response.data));
  }

  getMyShipmentsAll(pageSize: number = 100): Observable<ShipmentResponse[]> {
    return this.getMyShipmentsPage(1, pageSize).pipe(
      switchMap((firstPage) => {
        if (firstPage.totalPages <= 1) {
          return of(firstPage.data);
        }

        const pageRequests = Array.from({ length: firstPage.totalPages - 1 }, (_, index) =>
          this.getMyShipmentsPage(index + 2, pageSize)
        );

        return forkJoin(pageRequests).pipe(
          map((remainingPages) => [
            ...firstPage.data,
            ...remainingPages.flatMap((page) => page.data)
          ])
        );
      })
    );
  }

  getMyShipmentsPage(pageNumber: number, pageSize: number): Observable<PaginatedResponse<ShipmentResponse>> {
    return this.http.get<unknown>(`${this.apiUrl}/my`, {
      params: {
        pageNumber: String(pageNumber),
        pageSize: String(pageSize)
      }
    }).pipe(
      map((payload) => this.normalizePaginatedResponse<ShipmentResponse>(payload, pageNumber, pageSize))
    );
  }

  // Admin: get all shipments
  getAll(): Observable<ShipmentResponse[]> {
    return this.getAllPage(1, 5).pipe(map((response) => response.data));
  }

  getAllShipments(pageSize: number = 100): Observable<ShipmentResponse[]> {
    return this.getAllPage(1, pageSize).pipe(
      switchMap((firstPage) => {
        if (firstPage.totalPages <= 1) {
          return of(firstPage.data);
        }

        const pageRequests = Array.from({ length: firstPage.totalPages - 1 }, (_, index) =>
          this.getAllPage(index + 2, pageSize)
        );

        return forkJoin(pageRequests).pipe(
          map((remainingPages) => [
            ...firstPage.data,
            ...remainingPages.flatMap((page) => page.data)
          ])
        );
      })
    );
  }

  getAllPage(pageNumber: number, pageSize: number): Observable<PaginatedResponse<ShipmentResponse>> {
    return this.http.get<unknown>(this.apiUrl, {
      params: {
        pageNumber: String(pageNumber),
        pageSize: String(pageSize)
      }
    }).pipe(
      map((payload) => this.normalizePaginatedResponse<ShipmentResponse>(payload, pageNumber, pageSize))
    );
  }

  // Get by id
  getById(id: number): Observable<ShipmentResponse> {
    return this.http.get<ShipmentResponse>(`${this.apiUrl}/${id}`);
  }

  // Get by tracking number
  getByTrackingNumber(trackingNumber: string): Observable<ShipmentResponse> {
    return this.http.get<ShipmentResponse>(`${this.apiUrl}/tracking/${trackingNumber}`);
  }

  // Get shipments by customer (admin)
  getByCustomer(customerId: number): Observable<ShipmentResponse[]> {
    return this.http.get<ShipmentResponse[]>(`${this.apiUrl}/customer/${customerId}`);
  }

  create(shipment: CreateShipmentRequest): Observable<ShipmentResponse> {
    return this.http.post<ShipmentResponse>(this.apiUrl, shipment);
  }

  // Update shipment
  update(id: number, shipment: UpdateShipmentRequest): Observable<ShipmentResponse> {
    return this.http.put<ShipmentResponse>(`${this.apiUrl}/${id}`, shipment);
  }

  // Delete shipment (admin)
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Delete shipment (customer - only if not booked)
  deleteMyShipment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}/my`);
  }

  // ==== PACKAGES ====
  
  // Get packages for shipment
  getPackages(shipmentId: number): Observable<Package[]> {
    return this.http.get<Package[]>(`${this.apiUrl}/${shipmentId}/packages`);
  }

  // Add package to shipment
  addPackage(shipmentId: number, pkg: Package): Observable<Package> {
    return this.http.post<Package>(`${this.apiUrl}/${shipmentId}/packages`, pkg);
  }

  // Update package
  updatePackage(shipmentId: number, packageId: number, pkg: Package): Observable<Package> {
    return this.http.put<Package>(`${this.apiUrl}/${shipmentId}/packages/${packageId}`, pkg);
  }

  // Delete package
  deletePackage(shipmentId: number, packageId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${shipmentId}/packages/${packageId}`);
  }

  // ==== STATUS UPDATES ====

  // Status updates (admin)
  bookShipment(id: number, payload: BookShipmentRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/book`, payload);
  }

  pickupShipment(id: number, payload: ShipmentStatusUpdateRequest = {}): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/pickup`, payload);
  }

  inTransitShipment(id: number, payload: ShipmentStatusUpdateRequest = {}): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/in-transit`, payload);
  }

  outForDeliveryShipment(id: number, payload: ShipmentStatusUpdateRequest = {}): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/out-for-delivery`, payload);
  }

  deliverShipment(id: number, payload: ShipmentStatusUpdateRequest = {}): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/deliver`, payload);
  }

  // Schedule pickup
  schedulePickup(id: number, data: { pickupDate: string; notes: string }): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/schedule-pickup`, data);
  }

  getPickupDetails(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}/pickup-details`);
  }

  raiseIssue(id: number, payload: RaiseShipmentIssueRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/${id}/raise-issue`, payload);
  }

  // Calculate shipping rate
  calculateRate(data: RateRequest): Observable<RateResponse> {
    return this.http.post<RateResponse>(`${this.apiUrl}/calculate-rate`, data);
  }

  // Get available services
  getServices(): Observable<ShippingService[]> {
    return this.http.get<ShippingService[]>(`${this.apiUrl}/services`);
  }

  private normalizePaginatedResponse<T>(payload: unknown, pageNumber: number, pageSize: number): PaginatedResponse<T> {
    const normalizedPageNumber = Math.max(1, pageNumber);
    const normalizedPageSize = Math.max(1, pageSize);

    if (Array.isArray(payload)) {
      const startIndex = (normalizedPageNumber - 1) * normalizedPageSize;
      const items = payload.length > normalizedPageSize
        ? payload.slice(startIndex, startIndex + normalizedPageSize)
        : payload;

      return {
        data: items as T[],
        pageNumber: normalizedPageNumber,
        pageSize: normalizedPageSize,
        totalItems: payload.length,
        totalPages: Math.ceil(payload.length / normalizedPageSize),
        hasNextPage: false,
        hasPreviousPage: normalizedPageNumber > 1
      };
    }

    if (!payload || typeof payload !== 'object') {
      return {
        data: [],
        pageNumber: normalizedPageNumber,
        pageSize: normalizedPageSize,
        totalItems: 0,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false
      };
    }

    const result = payload as Record<string, unknown>;
    const currentPage = Number(result['pageNumber'] ?? normalizedPageNumber);
    const currentPageSize = Number(result['pageSize'] ?? normalizedPageSize);
    const data = Array.isArray(result['data']) ? (result['data'] as T[]) : [];
    const startIndex = (Math.max(1, currentPage) - 1) * Math.max(1, currentPageSize);
    const items = data.length > Math.max(1, currentPageSize)
      ? data.slice(startIndex, startIndex + Math.max(1, currentPageSize))
      : data;
    const totalItems = Number(result['totalItems'] ?? data.length);
    const totalPages = Number(result['totalPages'] ?? Math.ceil(totalItems / Math.max(currentPageSize, 1)));

    return {
      data: items,
      pageNumber: Math.max(1, currentPage),
      pageSize: Math.max(1, currentPageSize),
      totalItems,
      totalPages,
      hasNextPage: Boolean(result['hasNextPage'] ?? currentPage < totalPages),
      hasPreviousPage: Boolean(result['hasPreviousPage'] ?? currentPage > 1)
    };
  }
}
