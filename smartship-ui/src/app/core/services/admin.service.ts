import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, forkJoin, map, of, switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Address, ShipmentResponse } from '../../shared/models/shipment.model';
import { PaginatedResponse } from '../../shared/models/pagination.model';

/**
 * Admin API client used by the admin module (dashboard, hubs/locations, exception handling, monitoring, reports).
 *
 * Note: Some endpoints may return inconsistent payload shapes (service proxy vs direct shipment service),
 * so this service includes normalization logic for paginated results and shipment objects.
 */

// Matches backend DTOs from AdminService
// Dashboard Metrics DTOs
export interface StatMetric {
  count: number;
  percentageChange: number;
}

export interface DailyShipment {
  date: string;
  shipments: number;
}

export interface ServiceDistribution {
  serviceType: string;
  percentage: number;
}

export interface PerformanceTrend {
  month: string;
  onTimePercent: number;
  delayedPercent: number;
}

export interface DashboardMetrics {
  totalShipments: StatMetric;
  inTransit: StatMetric;
  delivered: StatMetric;
  exceptions: StatMetric;
  onTimeDeliveryPercent: number;
  avgDeliveryTimeDays: number;
  dailyShipments: DailyShipment[];
  serviceTypeDistribution: ServiceDistribution[];
  deliveryPerformanceTrend: PerformanceTrend[];
}

export interface HubResponse {
  hubId: number;
  name: string;
  address: string;
  contactNumber: string;
  managerName: string;
  email?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateHubRequest {
  name: string;
  address: string;
  contactNumber: string;
  managerName: string;
  email?: string;
  isActive: boolean;
}

export interface UpdateHubRequest extends CreateHubRequest {}

export interface LocationResponse {
  locationId: number;
  hubId: number;
  name: string;
  zipCode: string;
  isActive: boolean;
}

export interface CreateLocationRequest {
  hubId: number;
  name: string;
  zipCode: string;
  isActive: boolean;
}

export interface ExceptionRecord {
  exceptionId: number;
  shipmentId: number;
  exceptionType: string;
  description: string;
  status: string;
  createdAt: string;
  resolvedAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private readonly apiUrl = `${environment.apiBaseUrl}/admin/api/admin`;
  private readonly shipmentApiUrl = `${environment.apiBaseUrl}/shipment/api/shipments`;

  constructor(private http: HttpClient) {}

  // GET /admin/api/admin/dashboard
  getDashboard(): Observable<DashboardMetrics> {
    return this.http.get<DashboardMetrics>(`${this.apiUrl}/dashboard`);
  }

  // GET /admin/api/admin/statistics
  getDashboardMetrics(): Observable<DashboardMetrics> {
    return this.http.get<DashboardMetrics>(`${this.apiUrl}/statistics`);
  }

  // Hubs
  getHubs(): Observable<HubResponse[]> {
    return this.http.get<unknown>(`${this.apiUrl}/hubs`).pipe(
      map((payload) => this.normalizePaginatedResponse<HubResponse>(payload, 1, 50).data)
    );
  }

  getHub(id: number): Observable<HubResponse> {
    return this.http.get<HubResponse>(`${this.apiUrl}/hubs/${id}`);
  }

  createHub(hub: CreateHubRequest): Observable<HubResponse> {
    return this.http.post<HubResponse>(`${this.apiUrl}/hubs`, hub);
  }

  updateHub(id: number, hub: UpdateHubRequest): Observable<HubResponse> {
    return this.http.put<HubResponse>(`${this.apiUrl}/hubs/${id}`, hub);
  }

  deleteHub(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/hubs/${id}`);
  }

  // Locations
  getLocations(): Observable<LocationResponse[]> {
    return this.http.get<unknown>(`${this.apiUrl}/locations`).pipe(
      map((payload) => this.normalizePaginatedResponse<LocationResponse>(payload, 1, 50).data)
    );
  }

  createLocation(location: CreateLocationRequest): Observable<LocationResponse> {
    return this.http.post<LocationResponse>(`${this.apiUrl}/locations`, location);
  }

  updateLocation(id: number, location: CreateLocationRequest): Observable<LocationResponse> {
    return this.http.put<LocationResponse>(`${this.apiUrl}/locations/${id}`, location);
  }

  deleteLocation(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/locations/${id}`);
  }

  // issues
  getIssues(): Observable<ExceptionRecord[]> {
    return this.http.get<unknown>(`${this.apiUrl}/exceptions`).pipe(
      map((payload) => this.normalizePaginatedResponse<ExceptionRecord>(payload, 1, 50).data)
    );
  }

  resolveException(shipmentId: number, resolutionNotes: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/shipments/${shipmentId}/resolve`, { resolutionNotes });
  }

  delayShipment(shipmentId: number, reason: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/shipments/${shipmentId}/delay`, { reason });
  }

  returnShipment(shipmentId: number, reason: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/shipments/${shipmentId}/return`, { reason });
  }

  // Shipment monitoring — GET /admin/api/admin/shipments
  getShipments(): Observable<ShipmentResponse[]> {
    return this.getShipmentsPage(1, 5).pipe(map((response) => response.data));
  }

  getShipmentsAll(pageSize: number = 100): Observable<ShipmentResponse[]> {
    return this.getShipmentsPage(1, pageSize).pipe(
      switchMap((firstPage) => {
        if (firstPage.totalPages <= 1) {
          return of(firstPage.data);
        }

        const pageRequests = Array.from({ length: firstPage.totalPages - 1 }, (_, index) =>
          this.getShipmentsPage(index + 2, pageSize)
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

  getShipmentsPage(pageNumber: number, pageSize: number): Observable<PaginatedResponse<ShipmentResponse>> {
    return this.http.get<unknown>(`${this.apiUrl}/shipments`, {
      params: {
        pageNumber: String(pageNumber),
        pageSize: String(pageSize)
      }
    }).pipe(
      switchMap((adminResponse) => {
        const normalizedResponse = this.normalizeShipmentPaginatedResponse(adminResponse, pageNumber, pageSize);
        const adminRows = normalizedResponse.data;
        if (adminRows.length > 0 || this.isExplicitlyEmptyPayload(adminResponse)) {
          return of(normalizedResponse);
        }

        return this.http.get<unknown>(this.shipmentApiUrl, {
          params: {
            pageNumber: String(pageNumber),
            pageSize: String(pageSize)
          }
        }).pipe(
          map((shipmentResponse) => this.normalizeShipmentPaginatedResponse(shipmentResponse, pageNumber, pageSize))
        );
      }),
      catchError(() =>
        this.http.get<unknown>(this.shipmentApiUrl, {
          params: {
            pageNumber: String(pageNumber),
            pageSize: String(pageSize)
          }
        }).pipe(
          map((shipmentResponse) => this.normalizeShipmentPaginatedResponse(shipmentResponse, pageNumber, pageSize))
        )
      )
    );
  }

  getShipmentsByHub(hubId: number): Observable<any[]> {
    return this.getShipmentsByHubPage(hubId, 1, 5).pipe(map((response) => response.data));
  }

  getShipmentsByHubPage(hubId: number, pageNumber: number, pageSize: number): Observable<PaginatedResponse<ShipmentResponse>> {
    return this.http.get<unknown>(`${this.apiUrl}/shipments/hub/${hubId}`, {
      params: {
        pageNumber: String(pageNumber),
        pageSize: String(pageSize)
      }
    }).pipe(
      map((payload) => this.normalizeShipmentPaginatedResponse(payload, pageNumber, pageSize))
    );
  }

  private normalizeShipmentPayload(payload: unknown): ShipmentResponse[] {
    const parsedPayload = this.tryParseJsonString(payload);
    const rawArray = this.findBestArrayCandidate(parsedPayload);

    return rawArray
      .filter((entry): entry is Record<string, unknown> => !!entry && typeof entry === 'object')
      .map(entry => {
        const candidate = this.extractShipmentCandidate(entry, 0);
        if (!candidate) {
          return null;
        }

        const senderAddress = this.extractAddress(candidate, ['senderAddress', 'SenderAddress', 'originAddress', 'OriginAddress']);
        const receiverAddress = this.extractAddress(candidate, ['receiverAddress', 'ReceiverAddress', 'destinationAddress', 'DestinationAddress']);
        const rawPackages = candidate['packages'] ?? candidate['Packages'];
        const senderName = this.extractText(candidate, [
          'senderName', 'SenderName', 'senderFullName', 'SenderFullName',
          'shipperName', 'ShipperName'
        ]);
        const receiverName = this.extractText(candidate, [
          'receiverName', 'ReceiverName', 'receiverFullName', 'ReceiverFullName',
          'recipientName', 'RecipientName', 'consigneeName', 'ConsigneeName'
        ]);
        const customerName = this.extractText(candidate, ['customerName', 'CustomerName', 'customerFullName', 'CustomerFullName']);
        const serviceType = this.extractText(candidate, [
          'serviceType', 'ServiceType', 'service', 'Service'
        ]);

        return {
          shipmentId: Number(candidate['shipmentId'] ?? candidate['ShipmentId'] ?? candidate['id'] ?? candidate['Id'] ?? 0),
          trackingNumber: String(candidate['trackingNumber'] ?? candidate['TrackingNumber'] ?? candidate['trackingNo'] ?? candidate['TrackingNo'] ?? ''),
          customerId: Number(candidate['customerId'] ?? candidate['CustomerId'] ?? 0),
          customerName: customerName || undefined,
          senderName: senderName || undefined,
          receiverName: receiverName || undefined,
          serviceType: serviceType || undefined,
          estimatedCost: Number(candidate['estimatedCost'] ?? candidate['EstimatedCost'] ?? 0),
          status: this.normalizeShipmentStatus(candidate['status'] ?? candidate['Status']),
          totalWeight: Number(candidate['totalWeight'] ?? candidate['TotalWeight'] ?? candidate['weight'] ?? candidate['Weight'] ?? 0),
          createdAt: String(candidate['createdAt'] ?? candidate['CreatedAt'] ?? ''),
          senderAddress,
          receiverAddress,
          packages: Array.isArray(rawPackages) ? rawPackages as any[] : [],
          pickupSchedule: undefined
        } as ShipmentResponse;
      })
      .filter((shipment): shipment is ShipmentResponse => shipment !== null && (shipment.shipmentId > 0 || shipment.trackingNumber.length > 0));
  }

  private isExplicitlyEmptyPayload(payload: unknown): boolean {
    if (Array.isArray(payload) && payload.length === 0) {
      return true;
    }

    if (!payload || typeof payload !== 'object') {
      return false;
    }

    const root = payload as Record<string, unknown>;
    return root['count'] === 0 || root['Count'] === 0 || root['total'] === 0 || root['Total'] === 0;
  }

  private extractAddress(source: Record<string, unknown>, keys: string[]): Address {
    const firstMatch = keys
      .map(key => source[key])
      .find(value => value && typeof value === 'object' && !Array.isArray(value)) as Record<string, unknown> | undefined;

    return {
      street: String(firstMatch?.['street'] ?? firstMatch?.['Street'] ?? ''),
      city: String(firstMatch?.['city'] ?? firstMatch?.['City'] ?? 'Unknown'),
      state: String(firstMatch?.['state'] ?? firstMatch?.['State'] ?? ''),
      country: String(firstMatch?.['country'] ?? firstMatch?.['Country'] ?? ''),
      postalCode: String(firstMatch?.['postalCode'] ?? firstMatch?.['PostalCode'] ?? '')
    };
  }

  private normalizeShipmentStatus(status: unknown): ShipmentResponse['status'] {
    const statusByIndex: ShipmentResponse['status'][] = [
      'Draft', 'Booked', 'PickedUp', 'InTransit', 'OutForDelivery', 'Delivered'
    ];

    if (typeof status === 'number') {
      return statusByIndex[status] ?? 'Draft';
    }

    if (typeof status === 'string' && status.length > 0) {
      return status as ShipmentResponse['status'];
    }

    return 'Draft';
  }

  private extractText(source: Record<string, unknown>, keys: string[]): string {
    for (const key of keys) {
      const value = source[key];
      if (value !== undefined && value !== null) {
        const text = String(value).trim();
        if (text) {
          return text;
        }
      }
    }

    return '';
  }

  private tryParseJsonString(payload: unknown): unknown {
    if (typeof payload !== 'string') {
      return payload;
    }

    try {
      return JSON.parse(payload);
    } catch {
      return payload;
    }
  }

  private findBestArrayCandidate(payload: unknown): unknown[] {
    if (Array.isArray(payload)) {
      return payload;
    }

    if (!payload || typeof payload !== 'object') {
      return [];
    }

    const wrappers = ['shipments', 'items', 'data', 'result', 'results', 'value', '$values'];
    const root = payload as Record<string, unknown>;

    for (const key of wrappers) {
      const value = root[key];
      if (Array.isArray(value)) {
        return value;
      }

      if (value && typeof value === 'object') {
        const nestedArray = this.findBestArrayCandidate(value);
        if (nestedArray.length > 0) {
          return nestedArray;
        }
      }
    }

    const values = Object.values(root);
    const likelyDictionary = values.length > 0 && values.every(v => !!v && typeof v === 'object' && !Array.isArray(v));
    if (likelyDictionary) {
      return values;
    }

    for (const value of values) {
      const nestedArray = this.findBestArrayCandidate(value);
      if (nestedArray.length > 0) {
        return nestedArray;
      }
    }

    return [];
  }

  private extractShipmentCandidate(entry: Record<string, unknown>, depth: number): Record<string, unknown> | null {
    if (depth > 4) {
      return null;
    }

    if (this.looksLikeShipmentObject(entry)) {
      return entry;
    }

    for (const value of Object.values(entry)) {
      if (value && typeof value === 'object' && !Array.isArray(value)) {
        const nested = value as Record<string, unknown>;
        const nestedCandidate = this.extractShipmentCandidate(nested, depth + 1);
        if (nestedCandidate) {
          return nestedCandidate;
        }
      }
    }

    return null;
  }

  private looksLikeShipmentObject(value: Record<string, unknown>): boolean {
    return (
      'shipmentId' in value || 'ShipmentId' in value ||
      'trackingNumber' in value || 'TrackingNumber' in value ||
      'trackingNo' in value || 'TrackingNo' in value ||
      'senderAddress' in value || 'SenderAddress' in value ||
      'receiverAddress' in value || 'ReceiverAddress' in value
    );
  }

  getShipmentById(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/shipments/${id}`);
  }

  // Reports
  getReportsOverview(): Observable<any> {
    return this.http.get(`${this.apiUrl}/reports`);
  }

  getShipmentPerformance(): Observable<any> {
    return this.http.get(`${this.apiUrl}/reports/shipment-performance`);
  }

  getDeliverySLA(): Observable<any> {
    return this.http.get(`${this.apiUrl}/reports/delivery-sla`);
  }

  getRevenue(): Observable<any> {
    return this.http.get(`${this.apiUrl}/reports/revenue`);
  }

  getHubPerformance(): Observable<any> {
    return this.http.get(`${this.apiUrl}/reports/hub-performance`);
  }

  private normalizeShipmentPaginatedResponse(payload: unknown, pageNumber: number, pageSize: number): PaginatedResponse<ShipmentResponse> {
    const normalizedPageNumber = Math.max(1, pageNumber);
    const normalizedPageSize = Math.max(1, pageSize);

    if (!payload || typeof payload !== 'object' || Array.isArray(payload)) {
      const data = this.normalizeShipmentPayload(payload);
      return this.normalizePaginatedResponse<ShipmentResponse>(data, normalizedPageNumber, normalizedPageSize);
    }

    const result = payload as Record<string, unknown>;
    const mappedData = this.normalizeShipmentPayload(result['data'] ?? payload);
    const currentPage = Number(result['pageNumber'] ?? normalizedPageNumber);
    const currentPageSize = Number(result['pageSize'] ?? normalizedPageSize);
    const startIndex = (Math.max(1, currentPage) - 1) * Math.max(1, currentPageSize);
    const data = mappedData.length > Math.max(1, currentPageSize)
      ? mappedData.slice(startIndex, startIndex + Math.max(1, currentPageSize))
      : mappedData;
    const totalItems = Number(result['totalItems'] ?? mappedData.length);
    const totalPages = Number(result['totalPages'] ?? Math.ceil(totalItems / Math.max(currentPageSize, 1)));

    return {
      data,
      pageNumber: Math.max(1, currentPage),
      pageSize: Math.max(1, currentPageSize),
      totalItems,
      totalPages,
      hasNextPage: Boolean(result['hasNextPage'] ?? currentPage < totalPages),
      hasPreviousPage: Boolean(result['hasPreviousPage'] ?? currentPage > 1)
    };
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
