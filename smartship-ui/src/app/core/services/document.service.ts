import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DocumentResponse, DeliveryProofResponse } from '../../shared/models/document.model';
import { PaginatedResponse } from '../../shared/models/pagination.model';

@Injectable({
  providedIn: 'root'
})
/**
 * Document API client for shipment documents and delivery proof uploads/downloads.
 * Uses `FormData` for file endpoints and normalizes paginated payloads from different backend shapes.
 */
export class DocumentService {
  private readonly apiUrl = `${environment.apiBaseUrl}/document/api/documents`;

  constructor(private http: HttpClient) {}

  // General upload
  upload(shipmentId: number, file: File): Observable<DocumentResponse> {
    const formData = new FormData();
    formData.append('shipmentId', shipmentId.toString());
    formData.append('file', file);
    return this.http.post<DocumentResponse>(`${this.apiUrl}/upload`, formData);
  }

  // Upload invoice
  uploadInvoice(shipmentId: number, file: File): Observable<DocumentResponse> {
    const formData = new FormData();
    formData.append('shipmentId', shipmentId.toString());
    formData.append('file', file);
    return this.http.post<DocumentResponse>(`${this.apiUrl}/upload-invoice`, formData);
  }

  // Upload label
  uploadLabel(shipmentId: number, file: File): Observable<DocumentResponse> {
    const formData = new FormData();
    formData.append('shipmentId', shipmentId.toString());
    formData.append('file', file);
    return this.http.post<DocumentResponse>(`${this.apiUrl}/upload-label`, formData);
  }

  // Upload customs document
  uploadCustoms(shipmentId: number, file: File): Observable<DocumentResponse> {
    const formData = new FormData();
    formData.append('shipmentId', shipmentId.toString());
    formData.append('file', file);
    return this.http.post<DocumentResponse>(`${this.apiUrl}/upload-customs`, formData);
  }

  // Get document by ID
  getById(id: number): Observable<DocumentResponse> {
    return this.http.get<DocumentResponse>(`${this.apiUrl}/${id}`);
  }

  // Update document
  update(id: number, shipmentId: number, file: File): Observable<DocumentResponse> {
    const formData = new FormData();
    formData.append('shipmentId', shipmentId.toString());
    formData.append('file', file);
    return this.http.put<DocumentResponse>(`${this.apiUrl}/${id}`, formData);
  }

  // Delete document (admin only)
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Get documents by shipment
  getByShipment(shipmentId: number): Observable<DocumentResponse[]> {
    return this.getByShipmentPage(shipmentId, 1, 5).pipe(map((response) => response.data));
  }

  getByShipmentPage(shipmentId: number, pageNumber: number, pageSize: number): Observable<PaginatedResponse<DocumentResponse>> {
    return this.http.get<unknown>(`${this.apiUrl}/shipment/${shipmentId}`, {
      params: {
        pageNumber: String(pageNumber),
        pageSize: String(pageSize)
      }
    }).pipe(
      map((payload) => this.normalizePaginatedResponse<DocumentResponse>(payload, pageNumber, pageSize))
    );
  }

  // Get documents by customer
  getByCustomer(customerId: number): Observable<DocumentResponse[]> {
    return this.http.get<DocumentResponse[]>(`${this.apiUrl}/customer/${customerId}`);
  }

  // Upload delivery proof
  uploadDeliveryProof(shipmentId: number, file: File, signerName: string, notes: string): Observable<DeliveryProofResponse> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('signerName', signerName);
    formData.append('notes', notes);
    return this.http.post<DeliveryProofResponse>(`${this.apiUrl}/delivery-proof/${shipmentId}`, formData);
  }

  // Get delivery proof
  getDeliveryProof(shipmentId: number): Observable<DeliveryProofResponse> {
    return this.http.get<DeliveryProofResponse>(`${this.apiUrl}/delivery-proof/${shipmentId}`);
  }

  // Download file by stored file URL
  downloadFile(fileUrl: string): Observable<Blob> {
    const resolvedUrl = this.resolveDownloadUrl(fileUrl);
    return this.http.get(resolvedUrl, { responseType: 'blob' });
  }

  private resolveDownloadUrl(fileUrl: string): string {
    const raw = String(fileUrl ?? '').trim();
    if (!raw) return raw;

    if (/^https?:\/\//i.test(raw)) {
      return raw;
    }

    if (raw.startsWith('/document/')) {
      return raw;
    }

    if (raw.startsWith('/uploads/')) {
      return `${environment.apiBaseUrl}/document${raw}`;
    }

    if (raw.startsWith('uploads/')) {
      return `${environment.apiBaseUrl}/document/${raw}`;
    }

    return raw.startsWith('/') ? raw : `/${raw}`;
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
