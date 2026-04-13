import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { AdminService } from '../../../core/services/admin.service';
import { TrackingService } from '../../../core/services/tracking.service';
import { Address, ShipmentResponse, ShipmentStatus } from '../../../shared/models/shipment.model';
import { TrackingEvent, TrackingResponse } from '../../../shared/models/tracking.model';
import { LoaderComponent } from '../../../shared/components/loader/loader.component';
import { catchError, finalize, map } from 'rxjs/operators';
import { forkJoin, of } from 'rxjs';

@Component({
  selector: 'app-admin-shipments',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, LoaderComponent],
  templateUrl: './shipments.component.html',
  styleUrl: './shipments.component.css'
})
/**
 * Admin shipment listing screen.
 * Supports server-side pagination with client-side search/filtering and resolves a "current location" per shipment
 * by combining tracking info, timeline events, and hub directory enrichment.
 */
export class ShipmentsComponent implements OnInit {
  private adminService = inject(AdminService);
  private trackingService = inject(TrackingService);
  private route = inject(ActivatedRoute);
  private readonly defaultPageSize = 5;
  private readonly _shipments = signal<ShipmentResponse[]>([]);
  private readonly _filteredShipments = signal<ShipmentResponse[]>([]);
  private readonly _isLoading = signal(true);
  private readonly _error = signal('');
  private readonly _currentLocationByTracking = signal<Record<string, string>>({});
  private readonly _hubAddressByName = signal<Record<string, string>>({});
  private readonly _pageNumber = signal(1);
  private readonly _pageSize = signal(this.defaultPageSize);
  private readonly _totalItems = signal(0);
  private readonly _totalPages = signal(0);

  get shipments(): ShipmentResponse[] {
    return this._shipments();
  }

  get filteredShipments(): ShipmentResponse[] {
    return this._filteredShipments();
  }

  get isLoading(): boolean {
    return this._isLoading();
  }

  get error(): string {
    return this._error();
  }

  get pageNumber(): number {
    return this._pageNumber();
  }

  get totalPages(): number {
    return this._totalPages();
  }

  get totalItems(): number {
    return this._totalItems();
  }

  get hasPreviousPage(): boolean {
    return this.pageNumber > 1;
  }

  get hasNextPage(): boolean {
    return this.pageNumber < this.totalPages;
  }

  searchQuery = '';
  statusFilter = '';
  
  // Available statuses for filter
  statuses: ShipmentStatus[] = [
    'Draft', 'Booked', 'PickedUp', 'InTransit', 'OutForDelivery', 'Delivered'
  ];

  ngOnInit(): void {
    this.loadHubDirectory();
    this.route.queryParamMap.subscribe(params => {
      this.loadShipments(this.pageNumber);

      const statusUpdated = params.get('statusUpdated');
      if (statusUpdated === '1') {
        setTimeout(() => this.loadShipments(this.pageNumber), 2200);
      }
    });
  }

  private loadHubDirectory(): void {
    this.adminService.getHubs().pipe(
      catchError(() => of([]))
    ).subscribe((hubs: any[]) => {
      const mapByName: Record<string, string> = {};

      for (const hub of Array.isArray(hubs) ? hubs : []) {
        const name = String(hub?.name ?? '').trim();
        const address = String(hub?.address ?? '').trim();
        if (!name || !address) continue;
        mapByName[name.toLowerCase()] = address;
      }

      this._hubAddressByName.set(mapByName);
    });
  }

  loadShipments(pageNumber: number = 1): void {
    this._isLoading.set(true);
    this._error.set('');

    this.adminService.getShipmentsPage(pageNumber, this._pageSize()).pipe(
      catchError(err => {
        console.error('API Error:', err);
        if (err.status === 0) {
          this._error.set('Cannot connect to backend server. Make sure the Gateway (http://localhost:5000) is running.');
        } else if (err.status === 401) {
          this._error.set('Unauthorized. Please log in again.');
        } else if (err.status === 403) {
          this._error.set('Access denied. Admin role required.');
        } else {
          this._error.set(err.error?.message || 'Failed to load shipments from backend.');
        }
        return of({
          data: [] as ShipmentResponse[],
          pageNumber,
          pageSize: this._pageSize(),
          totalItems: 0,
          totalPages: 0,
          hasNextPage: false,
          hasPreviousPage: false
        });
      }),
      finalize(() => this._isLoading.set(false))
    ).subscribe(response => {
      const normalized = Array.isArray(response.data) ? response.data.map((s: ShipmentResponse) => ({
        ...s,
        senderAddress: this.ensureAddress(s?.senderAddress),
        receiverAddress: this.ensureAddress(s?.receiverAddress),
        packages: Array.isArray(s?.packages) ? s.packages : []
      })) : [];

      const sorted = [...normalized].sort((a, b) => {
        const aTime = this.getShipmentCreatedAtTime(a);
        const bTime = this.getShipmentCreatedAtTime(b);

        if (aTime !== bTime) {
          return bTime - aTime;
        }

        return (b.shipmentId ?? 0) - (a.shipmentId ?? 0);
      });

      this._shipments.set(sorted);
      this._pageNumber.set(response.pageNumber);
      this._totalItems.set(response.totalItems);
      this._totalPages.set(response.totalPages);
      this.applyFilters();
      this.loadCurrentLocations(sorted);
    });
  }

  private getShipmentCreatedAtTime(shipment: ShipmentResponse): number {
    const createdAt = new Date(shipment.createdAt ?? '');
    return Number.isNaN(createdAt.getTime()) ? 0 : createdAt.getTime();
  }

  goToPage(pageNumber: number): void {
    if (pageNumber < 1 || pageNumber > this.totalPages || pageNumber === this.pageNumber) {
      return;
    }
    this.loadShipments(pageNumber);
  }

  private loadCurrentLocations(shipments: ShipmentResponse[]): void {
    const rows = Array.isArray(shipments) ? shipments : [];
    const shipmentsByTracking = rows
      .filter(s => !!String(s?.trackingNumber ?? '').trim())
      .reduce<Record<string, ShipmentResponse>>((acc, shipment) => {
        const trackingNumber = String(shipment.trackingNumber ?? '').trim();
        if (trackingNumber && !acc[trackingNumber]) {
          acc[trackingNumber] = shipment;
        }
        return acc;
      }, {});

    const trackingNumbers = Object.keys(shipmentsByTracking);

    if (trackingNumbers.length === 0) {
      this._currentLocationByTracking.set({});
      return;
    }

    const requests = trackingNumbers.map(trackingNumber =>
      forkJoin({
        tracking: this.trackingService.getTrackingInfo(trackingNumber).pipe(
          catchError(() => of(null as TrackingResponse | null))
        ),
        timeline: this.trackingService.getTimeline(trackingNumber).pipe(
          catchError(() => of([] as TrackingEvent[]))
        )
      }).pipe(
        map(({ tracking, timeline }) => ({
          trackingNumber,
          location: this.resolveCurrentLocation(shipmentsByTracking[trackingNumber], tracking, timeline)
        })),
        catchError(() => of({ trackingNumber, location: '' }))
      )
    );

    forkJoin(requests).subscribe(results => {
      const locationByTracking: Record<string, string> = {};

      for (const result of results) {
        if (result.location) {
          locationByTracking[result.trackingNumber] = result.location;
        }
      }

      this._currentLocationByTracking.set(locationByTracking);
    });
  }

  private ensureAddress(address: Address | null | undefined): Address {
    return {
      street: address?.street ?? '',
      city: address?.city ?? 'Unknown',
      state: address?.state ?? '',
      country: address?.country ?? '',
      postalCode: address?.postalCode ?? ''
    };
  }

  applyFilters(): void {
    let filtered = [...this._shipments()];

    if (this.searchQuery) {
      const q = this.searchQuery.toLowerCase();
      filtered = filtered.filter(s => 
        (s.trackingNumber && s.trackingNumber.toLowerCase().includes(q)) ||
        this.getSenderName(s).toLowerCase().includes(q) ||
        this.getReceiverName(s).toLowerCase().includes(q)
      );
    }

    if (this.statusFilter) {
      filtered = filtered.filter(s => this.normalizeStatus(s.status) === this.statusFilter);
    }

    this._filteredShipments.set(filtered);
  }

  // Map numeric enum values from backend (when JsonStringEnumConverter is not applied)
  private readonly statusByIndex: ShipmentStatus[] = [
    'Draft', 'Booked', 'PickedUp', 'InTransit', 'OutForDelivery', 'Delivered'
  ];

  normalizeStatus(status: unknown): ShipmentStatus {
    if (typeof status === 'number') {
      return this.statusByIndex[status] ?? 'Draft';
    }
    if (typeof status === 'string' && status.length > 0) {
      return status as ShipmentStatus;
    }
    return 'Draft';
  }

  getStatusClass(status: unknown): string {
    const normalized = this.normalizeStatus(status);
    const map: Record<ShipmentStatus, string> = {
      'Draft': 'status-draft',
      'Booked': 'status-booked',
      'PickedUp': 'status-pickedup',
      'InTransit': 'status-intransit',
      'OutForDelivery': 'status-outfordelivery',
      'Delivered': 'status-delivered'
    };
    return map[normalized] ?? 'status-draft';
  }

  formatStatus(status: unknown): string {
    const normalized = this.normalizeStatus(status);
    return normalized.replace(/([A-Z])/g, ' $1').trim();
  }

  exportReport(): void {
    const rows = this._filteredShipments();
    const csvParts: string[] = [];
    csvParts.push('SmartShip Shipments Export');
    csvParts.push(`Generated At,${new Date().toISOString()}`);
    csvParts.push('');
    csvParts.push('Tracking #,Shipment ID,Sender City,Sender State,Receiver City,Receiver State,Service,Status,Weight (kg),Created At');
    for (const s of rows) {
      const service = this.getServiceType(s);
      const id = `SH${s.shipmentId.toString().padStart(3, '0')}`;
      csvParts.push([
        s.trackingNumber,
        id,
        s.senderAddress?.city ?? '',
        s.senderAddress?.state ?? '',
        s.receiverAddress?.city ?? '',
        s.receiverAddress?.state ?? '',
        service,
        this.formatStatus(s.status),
        s.totalWeight,
        s.createdAt
      ].map(v => `"${String(v).replace(/"/g, '""')}"`).join(','));
    }
    const blob = new Blob([csvParts.join('\n')], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `smartship-shipments-${new Date().toISOString().slice(0, 10)}.csv`;
    link.click();
    URL.revokeObjectURL(link.href);
  }

  getCurrentLocation(shipment: ShipmentResponse): string {
    const trackingNumber = String(shipment?.trackingNumber ?? '').trim();
    const location = trackingNumber ? this._currentLocationByTracking()[trackingNumber] : '';
    return this.enrichHubLocation(location) || 'Not available';
  }

  getSenderName(shipment: ShipmentResponse): string {
    const shipmentSenderName = String(shipment?.senderName ?? '').trim();

    if (shipmentSenderName) {
      return shipmentSenderName;
    }

    return 'Name not available';
  }

  getReceiverName(shipment: ShipmentResponse): string {
    const shipmentReceiverName = String(shipment?.receiverName ?? '').trim();
    if (shipmentReceiverName) {
      return shipmentReceiverName;
    }

    return 'Name not available';
  }

  getServiceType(shipment: ShipmentResponse): string {
    const serviceType = String(shipment?.serviceType ?? '').trim();
    return serviceType || 'Standard';
  }

  private resolveCurrentLocation(shipment: ShipmentResponse, tracking: TrackingResponse | null, timeline: TrackingEvent[]): string {
    const status = this.normalizeStatus(shipment?.status);
    if (status === 'Delivered') {
      return this.enrichHubLocation(this.formatAddressSingleLine(shipment?.receiverAddress)) || 'Not available';
    }

    const timelineEvents = (Array.isArray(timeline) ? timeline : [])
      .filter(event => !!String(event?.location ?? '').trim())
      .sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
    const latestTimelineLocation = timelineEvents[0]?.location;
    const statusAwareFallback = this.getStatusAwareFallbackLocation(shipment);
    const resolved = latestTimelineLocation || tracking?.currentLocation || statusAwareFallback;

    return this.enrichHubLocation(this.normalizeLocationText(resolved));
  }

  private getStatusAwareFallbackLocation(shipment: ShipmentResponse): string {
    const status = this.normalizeStatus(shipment?.status);

    if (status === 'Delivered') {
      return this.formatAddressSingleLine(shipment?.receiverAddress);
    }

    if (status === 'OutForDelivery') {
      const receiver = this.formatAddressSingleLine(shipment?.receiverAddress);
      return receiver || this.formatAddressSingleLine(shipment?.senderAddress);
    }

    if (status === 'Draft' || status === 'Booked' || status === 'PickedUp') {
      return this.formatAddressSingleLine(shipment?.senderAddress);
    }

    return this.formatAddressSingleLine(shipment?.senderAddress);
  }

  private formatAddressSingleLine(address: Address | null | undefined): string {
    if (!address) {
      return '';
    }

    return [address.street, address.city, address.state, address.country]
      .map(part => String(part ?? '').trim())
      .filter(Boolean)
      .join(', ');
  }

  private enrichHubLocation(location: unknown): string {
    const text = String(location ?? '').trim();
    if (!text) {
      return '';
    }

    if (text.includes(' - ') || text.includes(', ')) {
      return text;
    }

    const hubAddress = this._hubAddressByName()[text.toLowerCase()];
    if (hubAddress) {
      return `${text}, ${hubAddress}`;
    }

    return text;
  }

  private normalizeLocationText(value: unknown): string {
    const text = String(value ?? '').trim();
    if (!text) {
      return '';
    }

    const parts = text.split(',').map(part => part.trim());
    if (parts.length >= 2) {
      const lat = Number(parts[0]);
      const lng = Number(parts[1]);

      if (Number.isFinite(lat) && Number.isFinite(lng) && lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180) {
        return `${lat.toFixed(5)}, ${lng.toFixed(5)}`;
      }
    }

    return text;
  }
}
