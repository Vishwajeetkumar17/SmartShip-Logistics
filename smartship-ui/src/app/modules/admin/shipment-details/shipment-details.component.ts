import { Component, inject, OnInit, signal, ViewChild, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ShipmentService } from '../../../core/services/shipment.service';
import { ShipmentResponse, Package } from '../../../shared/models/shipment.model';
import { AdminService, HubResponse } from '../../../core/services/admin.service';
import { TrackingService } from '../../../core/services/tracking.service';
import { TrackingEvent } from '../../../shared/models/tracking.model';
import { catchError, forkJoin, of } from 'rxjs';

@Component({
  selector: 'app-admin-shipment-details',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './shipment-details.component.html',
  styleUrl: './shipment-details.component.css'
})
/**
 * Admin shipment details screen.
 * Supports package CRUD, status transitions (book/pickup/transit/out-for-delivery/deliver), hub selection with typeahead,
 * and tracking timeline synchronization (including resilient fallback writes).
 */
export class ShipmentDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private shipmentService = inject(ShipmentService);
  private adminService = inject(AdminService);
  private trackingService = inject(TrackingService);

  @ViewChild('invoiceContent') invoiceContent!: ElementRef;
  @ViewChild('invoiceActions') invoiceActions?: ElementRef;

  shipmentId = 0;

  private readonly _shipment = signal<ShipmentResponse | null>(null);
  private readonly _packages = signal<Package[]>([]);
  private readonly _isLoading = signal(true);
  private readonly _error = signal('');
  private readonly _success = signal('');
  private readonly _isUpdatingStatus = signal(false);
  private readonly _isRemovingPackage = signal(false);
  private readonly _showAddPackageForm = signal(false);
  private readonly _isSubmittingPackage = signal(false);
  private readonly _hubs = signal<HubResponse[]>([]);
  private readonly _selectedHubId = signal<number | null>(null);
  private readonly _hubSearchText = signal('');
  private readonly _isHubDropdownOpen = signal(false);
  private readonly _highlightedHubIndex = signal(-1);
  private readonly _trackingTimeline = signal<TrackingEvent[]>([]);
  private readonly _showInvoiceMenu = signal(false);
  private readonly _isTimelineSyncing = signal(false);

  get shipment(): ShipmentResponse | null { return this._shipment(); }
  get packages(): Package[] { return this._packages(); }
  get isLoading(): boolean { return this._isLoading(); }
  get error(): string { return this._error(); }
  get success(): string { return this._success(); }
  get isUpdatingStatus(): boolean { return this._isUpdatingStatus(); }
  get isRemovingPackage(): boolean { return this._isRemovingPackage(); }
  get showAddPackageForm(): boolean { return this._showAddPackageForm(); }
  get isSubmittingPackage(): boolean { return this._isSubmittingPackage(); }
  get hubs(): HubResponse[] { return this._hubs(); }
  get hubSearchText(): string { return this._hubSearchText(); }
  get isHubDropdownOpen(): boolean { return this._isHubDropdownOpen(); }
  get highlightedHubIndex(): number { return this._highlightedHubIndex(); }
  get selectedHubId(): number | null { return this._selectedHubId(); }
  get selectedHubDisplayText(): string { return this.getSelectedHubDisplayText(); }
  get filteredHubs(): HubResponse[] {
    const query = this._hubSearchText().trim().toLowerCase();
    if (!query) return this._hubs();

    return this._hubs().filter(hub => {
      const name = String(hub.name ?? '').toLowerCase();
      const address = String(hub.address ?? '').toLowerCase();
      return name.includes(query) || address.includes(query);
    });
  }
  get trackingTimeline(): TrackingEvent[] { return this._trackingTimeline(); }
  get showInvoiceMenu(): boolean { return this._showInvoiceMenu(); }
  get isTimelineSyncing(): boolean { return this._isTimelineSyncing(); }

  get currentShipmentStatus(): string {
    return this.normalizeStatusValue(this._shipment()?.status);
  }

  get isHubSelectionDisabled(): boolean {
    return this._isUpdatingStatus() || this.currentShipmentStatus === 'Delivered';
  }

  get currentStatusHubLocation(): string {
    const shipment = this._shipment();
    const currentStatus = this.currentShipmentStatus;
    if (!shipment || !currentStatus) {
      return '';
    }

    const latestMatchingEvent = this._trackingTimeline().find(event =>
      this.normalizeStatusValue(event.status) === currentStatus && String(event.location ?? '').trim().length > 0
    );

    if (latestMatchingEvent?.location) {
      return latestMatchingEvent.location;
    }

    if (currentStatus === 'PickedUp') {
      return this.formatAddressSingleLine(shipment.senderAddress) || '';
    }

    if (currentStatus === 'Delivered') {
      return this.formatAddressSingleLine(shipment.receiverAddress) || '';
    }

    return '';
  }

  // New package form state
  newPackage: Package = { weight: 0, length: 0, width: 0, height: 0, description: '' };
  newPackageFieldErrors: Record<'weight' | 'length' | 'width' | 'height' | 'description', string> = {
    weight: '',
    length: '',
    width: '',
    height: '',
    description: ''
  };

  ngOnInit(): void {
    this.loadHubs();

    this.route.params.subscribe(params => {
      this.shipmentId = +params['id'];
      if (this.shipmentId) {
        this.loadShipmentData();
      }
    });
  }

  loadHubs(): void {
    this.adminService.getHubs().subscribe({
      next: (data) => {
        const activeHubs = (Array.isArray(data) ? data : []).filter(hub => hub.isActive);
        this._hubs.set(activeHubs);
        // Only set default hub if BOTH hubId and search text are empty
        if (!this._selectedHubId() && !this._hubSearchText() && activeHubs.length > 0) {
          const firstHub = activeHubs[0];
          this._selectedHubId.set(firstHub.hubId);
          this._hubSearchText.set(this.getHubDisplayText(firstHub));
        }
      },
      error: () => {
        this._hubs.set([]);
      }
    });
  }

  loadShipmentData(): void {
    this._isLoading.set(true);
    this._error.set('');

    this.shipmentService.getById(this.shipmentId).subscribe({
      next: (data) => {
        if (data) {
           const normalized = {
             ...data,
             senderAddress: data.senderAddress || { street: '', city: 'Unknown', state: '', country: '', postalCode: '' },
             receiverAddress: data.receiverAddress || { street: '', city: 'Unknown', state: '', country: '', postalCode: '' },
             packages: data.packages || []
           };
           this._shipment.set(normalized);
           this.loadTrackingTimeline(normalized.trackingNumber);
        }
        this.loadPackages();
      },
      error: (err) => {
        console.error('API Error:', err);
        this._error.set('Failed to load shipment details. Make sure the backend is running.');
        this._isLoading.set(false);
      }
    });
  }

  loadTrackingTimeline(trackingNumber: string): void {
    if (!trackingNumber?.trim()) {
      this._trackingTimeline.set([]);
      this._isTimelineSyncing.set(false);
      return;
    }

    this._isTimelineSyncing.set(true);

    this.trackingService.getTimeline(trackingNumber).subscribe({
      next: (events) => {
        const sorted = (Array.isArray(events) ? events : [])
          .filter((event): event is TrackingEvent => !!event)
          .sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
        const merged = this.mergeTimelineEvents(this._trackingTimeline(), sorted);
        this._trackingTimeline.set(this.ensureTimelineIncludesCurrentStatus(merged, trackingNumber));
        this.backfillTimelineStagesIfMissing(trackingNumber, sorted);
        this._isTimelineSyncing.set(false);
      },
      error: () => {
        this._trackingTimeline.set(this.buildSyntheticTimeline(trackingNumber));
        this.backfillTimelineStagesIfMissing(trackingNumber, []);
        this._isTimelineSyncing.set(false);
      }
    });
  }

  loadPackages(): void {
    this.shipmentService.getPackages(this.shipmentId).subscribe({
      next: (data) => {
        this._packages.set(data || []);
        this._isLoading.set(false);
      },
      error: (err) => {
        console.error('Packages Error:', err);
        this._packages.set(this._shipment()?.packages || []);
        this._isLoading.set(false);
      }
    });
  }

  // ==== PACKAGES ====
  
  toggleAddPackageForm(): void {
    this._showAddPackageForm.set(!this._showAddPackageForm());
    if (!this._showAddPackageForm()) {
      this.resetNewPackage();
    }
  }

  resetNewPackage(): void {
    this.newPackage = { weight: 0, length: 0, width: 0, height: 0, description: '' };
    this.newPackageFieldErrors = {
      weight: '',
      length: '',
      width: '',
      height: '',
      description: ''
    };
  }

  submitNewPackage(): void {
    this._error.set('');
    this._success.set('');
    this.newPackageFieldErrors = {
      weight: '',
      length: '',
      width: '',
      height: '',
      description: ''
    };

    if (!this.newPackage.weight || this.newPackage.weight <= 0) {
      this.newPackageFieldErrors.weight = 'Weight must be greater than 0.';
    }

    if (!this.newPackage.length || this.newPackage.length <= 0) {
      this.newPackageFieldErrors.length = 'Length must be greater than 0.';
    }

    if (!this.newPackage.width || this.newPackage.width <= 0) {
      this.newPackageFieldErrors.width = 'Width must be greater than 0.';
    }

    if (!this.newPackage.height || this.newPackage.height <= 0) {
      this.newPackageFieldErrors.height = 'Height must be greater than 0.';
    }

    if (!this.newPackage.description?.trim()) {
      this.newPackageFieldErrors.description = 'Description is required.';
    }

    if (Object.values(this.newPackageFieldErrors).some(message => !!message)) {
      return;
    }

    this._isSubmittingPackage.set(true);

    this.shipmentService.addPackage(this.shipmentId, this.newPackage).subscribe({
      next: (addedPkg) => {
        this._success.set('Package added successfully!');
        this._packages.set([...this._packages(), addedPkg]);
        this.toggleAddPackageForm();
        this._isSubmittingPackage.set(false);
        setTimeout(() => this._success.set(''), 3000);
      },
      error: (err) => {
        this._error.set(err.error?.message || 'Failed to add package.');
        this._isSubmittingPackage.set(false);
      }
    });
  }

  onNewPackageFieldChange(field: 'weight' | 'length' | 'width' | 'height' | 'description'): void {
    this.newPackageFieldErrors[field] = '';
    this._error.set('');
  }

  removePackage(packageId: number): void {
    if (!confirm('Are you sure you want to remove this package?')) return;
    
    this._isRemovingPackage.set(true);
    this._error.set('');
    this._success.set('');

    this.shipmentService.deletePackage(this.shipmentId, packageId).subscribe({
      next: () => {
        this._packages.set(this._packages().filter((p: Package) => p.id !== packageId));
        this._success.set('Package removed.');
        this._isRemovingPackage.set(false);
        setTimeout(() => this._success.set(''), 3000);
      },
      error: () => {
        this._error.set('Failed to remove package.');
        this._isRemovingPackage.set(false);
      }
    });
  }

  // ==== STATUS ====

  canBook(): boolean {
    return this.currentShipmentStatus === 'Draft';
  }

  canPickup(): boolean {
    return this.currentShipmentStatus === 'Booked';
  }

  canTransit(): boolean {
    return this.currentShipmentStatus === 'PickedUp' || this.currentShipmentStatus === 'InTransit';
  }

  canOutForDelivery(): boolean {
    return this.currentShipmentStatus === 'InTransit';
  }

  canDeliver(): boolean {
    return this.currentShipmentStatus === 'OutForDelivery';
  }

  isStatusDone(status: string): boolean {
    const order = ['Draft', 'Booked', 'PickedUp', 'InTransit', 'OutForDelivery', 'Delivered'];
    const currentIndex = order.indexOf(this.currentShipmentStatus);
    const stageIndex = order.indexOf(status);
    if (currentIndex < 0 || stageIndex < 0) {
      return false;
    }

    return currentIndex > stageIndex;
  }

  updateState(action: 'book' | 'pickup' | 'transit' | 'outfordelivery' | 'deliver'): void {
    const selectedHub = this._hubs().find(hub => hub.hubId === this._selectedHubId()) ?? null;
    const selectedHubLocation = selectedHub
      ? `${selectedHub.name}${selectedHub.address ? `, ${selectedHub.address}` : ''}`
      : '';
    const shipment = this._shipment();
    const pickupLocation = this.formatAddressSingleLine(shipment?.senderAddress) || undefined;
    const deliveryLocation = this.formatAddressSingleLine(shipment?.receiverAddress) || undefined;
    const requiresOperationalHub = action === 'book' || action === 'transit' || action === 'outfordelivery';
    const fallbackLocation = action === 'pickup'
      ? pickupLocation
      : action === 'deliver'
        ? deliveryLocation
        : selectedHubLocation || undefined;

    if (requiresOperationalHub && !selectedHub) {
      this._error.set(`Please select an operational hub for ${this.getActionLabel(action)}. Pickup and Delivery use sender/receiver addresses automatically.`);
      return;
    }

    this._isUpdatingStatus.set(true);
    this._error.set('');
    this._success.set('');

    let ob$;
    if (action === 'book') {
      ob$ = this.shipmentService.bookShipment(this.shipmentId, {
        hubName: selectedHub!.name,
        hubAddress: selectedHub?.address
      });
    }
    else if (action === 'pickup') ob$ = this.shipmentService.pickupShipment(this.shipmentId, { hubLocation: pickupLocation });
    else if (action === 'transit') ob$ = this.shipmentService.inTransitShipment(this.shipmentId, { hubLocation: selectedHubLocation || undefined });
    else if (action === 'outfordelivery') ob$ = this.shipmentService.outForDeliveryShipment(this.shipmentId, { hubLocation: selectedHubLocation || undefined });
    else ob$ = this.shipmentService.deliverShipment(this.shipmentId, { hubLocation: deliveryLocation });

    ob$.subscribe({
      next: () => {
        const shipment = this._shipment();
        if (shipment) {
          const updatedStatus = this.getActionStatus(action);
          const optimisticLocation = String(fallbackLocation ?? this.getDefaultLocationByStatus(updatedStatus) ?? '').trim() || 'Location not available';
          this._shipment.set({
            ...shipment,
            status: updatedStatus
          });

          const optimisticEvent: TrackingEvent = {
            trackingNumber: shipment.trackingNumber,
            status: updatedStatus,
            location: optimisticLocation,
            description: this.getDefaultDescription(updatedStatus),
            timestamp: new Date().toISOString()
          };

          const mergedTimeline = [optimisticEvent, ...this._trackingTimeline()]
            .sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
          this._trackingTimeline.set(mergedTimeline);

          this.persistTrackingEventFallback(shipment.trackingNumber, updatedStatus, fallbackLocation);

          // Refresh from tracking service after a short delay to absorb async processing latency.
          this._isTimelineSyncing.set(true);
          setTimeout(() => this.loadTrackingTimeline(shipment.trackingNumber), 900);
        }

        this._success.set(`Status updated to ${this.getActionLabel(action)}.`);
        this._isUpdatingStatus.set(false);
        setTimeout(() => this._success.set(''), 3000);
      },
      error: (err) => {
        this._error.set(err.error?.message || 'Failed to update status.');
        this._isUpdatingStatus.set(false);
      }
    });
  }

  private persistTrackingEventFallback(
    trackingNumber: string,
    status: ShipmentResponse['status'],
    location?: string
  ): void {
    const tracking = String(trackingNumber ?? '').trim();
    if (!tracking) {
      return;
    }

    const resolvedLocation = String(location ?? this.getDefaultLocationByStatus(status) ?? '').trim() || 'Location not available';

    const payload: TrackingEvent = {
      trackingNumber: tracking,
      status,
      location: resolvedLocation,
      description: this.getDefaultDescription(status),
      timestamp: new Date().toISOString()
    };

    this.trackingService.addEvent(payload).subscribe({
      next: () => {
        // no-op: this is a resilience write to preserve stage history
      },
      error: () => {
        // Retry once in case tracking service is briefly unavailable.
        setTimeout(() => {
          this.trackingService.addEvent(payload).subscribe({
            next: () => {
              // no-op
            },
            error: () => {
              // ignore fallback failures; shipment status is already updated
            }
          });
        }, 600);
      }
    });
  }

  private mergeTimelineEvents(currentEvents: TrackingEvent[], serverEvents: TrackingEvent[]): TrackingEvent[] {
    const combined = [
      ...(Array.isArray(currentEvents) ? currentEvents : []),
      ...(Array.isArray(serverEvents) ? serverEvents : [])
    ];

    const unique = new Map<string, TrackingEvent>();
    for (const event of combined) {
      if (!event) {
        continue;
      }

      const key = [
        String(event.eventId ?? ''),
        this.normalizeStatusValue(event.status),
        String(event.location ?? '').trim().toLowerCase(),
        String(event.description ?? '').trim().toLowerCase(),
        new Date(event.timestamp).toISOString()
      ].join('|');

      if (!unique.has(key)) {
        unique.set(key, event);
      }
    }

    return Array.from(unique.values())
      .sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
  }

  private backfillTimelineStagesIfMissing(trackingNumber: string, existingEvents: TrackingEvent[]): void {
    const shipment = this._shipment();
    const currentStatus = this.currentShipmentStatus;
    const currentRank = this.getStatusRank(currentStatus);
    if (!shipment || currentRank <= 1) {
      return;
    }

    const existingStatusSet = new Set(
      (Array.isArray(existingEvents) ? existingEvents : [])
        .map(event => this.normalizeStatusValue(event.status))
        .filter(Boolean)
    );

    const stagedOrder: ShipmentResponse['status'][] = ['Booked', 'PickedUp', 'InTransit', 'OutForDelivery', 'Delivered'];
    const missingStages = stagedOrder
      .filter(stage => this.getStatusRank(stage) <= currentRank)
      .filter(stage => !existingStatusSet.has(stage));

    if (missingStages.length === 0) {
      return;
    }

    const validTimes = (Array.isArray(existingEvents) ? existingEvents : [])
      .map(event => new Date(event.timestamp).getTime())
      .filter(ms => Number.isFinite(ms) && ms > 0);
    const earliestExistingMs = validTimes.length > 0 ? Math.min(...validTimes) : Number.NaN;

    const createdAtMs = new Date(shipment.createdAt ?? '').getTime();
    const fallbackBaseMs = Number.isFinite(createdAtMs) && createdAtMs > 0
      ? createdAtMs + 60_000
      : Date.now() - 10 * 60_000;

    const anchorMs = Number.isFinite(earliestExistingMs)
      ? earliestExistingMs
      : fallbackBaseMs;

    const inserts = missingStages.map((stage, index) => {
      const minutesBeforeAnchor = missingStages.length - index;
      const timestampMs = anchorMs - (minutesBeforeAnchor * 60_000);
      return this.trackingService.addEvent({
        trackingNumber,
        status: stage,
        location: this.getDefaultLocationByStatus(stage),
        description: this.getDefaultDescription(stage),
        timestamp: new Date(timestampMs).toISOString()
      }).pipe(catchError(() => of(null)));
    });

    forkJoin(inserts).subscribe({
      next: () => {
        this.trackingService.getTimeline(trackingNumber).subscribe({
          next: (events) => {
            const sorted = (Array.isArray(events) ? events : [])
              .filter((event): event is TrackingEvent => !!event)
              .sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
            this._trackingTimeline.set(this.ensureTimelineIncludesCurrentStatus(sorted, trackingNumber));
          },
          error: () => {
            // keep currently displayed timeline if refresh fails
          }
        });
      }
    });
  }

  onHubChange(value: unknown): void {
    if (value === null || value === undefined || value === '') {
      this._selectedHubId.set(null);
      return;
    }

    const parsed = Number(value);
    this._selectedHubId.set(Number.isFinite(parsed) && parsed > 0 ? parsed : null);
  }

  onHubSearchTextChange(value: unknown): void {
    if (this.isHubSelectionDisabled) {
      return;
    }

    const text = String(value ?? '');
    this._hubSearchText.set(text);
    this._isHubDropdownOpen.set(true);
    this.syncHighlightedHubIndex();

    const selectedHubId = this._selectedHubId();
    if (selectedHubId && this.getSelectedHubDisplayText() !== text.trim()) {
      this._selectedHubId.set(null);
    }
  }

  openHubDropdown(): void {
    if (this.isHubSelectionDisabled) {
      return;
    }

    this._isHubDropdownOpen.set(true);
    this.syncHighlightedHubIndex();
  }

  closeHubDropdownSoon(): void {
    if (this.isHubSelectionDisabled) {
      this._isHubDropdownOpen.set(false);
      this._highlightedHubIndex.set(-1);
      return;
    }

    setTimeout(() => {
      this._isHubDropdownOpen.set(false);
      this._highlightedHubIndex.set(-1);
    }, 120);
  }

  selectHub(hub: HubResponse): void {
    if (this.isHubSelectionDisabled) {
      return;
    }

    this._selectedHubId.set(hub.hubId);
    this._hubSearchText.set(this.getHubDisplayText(hub));
    this._isHubDropdownOpen.set(false);
    this._highlightedHubIndex.set(-1);
  }

  clearHubSearch(): void {
    if (this.isHubSelectionDisabled) {
      return;
    }

    this._hubSearchText.set('');
    this._selectedHubId.set(null);
    this._isHubDropdownOpen.set(true);
    this.syncHighlightedHubIndex();
  }

  onHubInputKeydown(event: KeyboardEvent): void {
    if (this.isHubSelectionDisabled) {
      return;
    }

    if (!this._isHubDropdownOpen()) {
      if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
        event.preventDefault();
        this._isHubDropdownOpen.set(true);
        this.syncHighlightedHubIndex();
      }
      return;
    }

    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.moveHubHighlight(1);
      return;
    }

    if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.moveHubHighlight(-1);
      return;
    }

    if (event.key === 'Enter') {
      const index = this._highlightedHubIndex();
      if (index >= 0 && index < this.filteredHubs.length) {
        event.preventDefault();
        this.selectHub(this.filteredHubs[index]);
      }
      return;
    }

    if (event.key === 'Escape') {
      event.preventDefault();
      this._isHubDropdownOpen.set(false);
      this._highlightedHubIndex.set(-1);
    }
  }

  private moveHubHighlight(step: number): void {
    const hubs = this.filteredHubs;
    if (hubs.length === 0) {
      this._highlightedHubIndex.set(-1);
      return;
    }

    const current = this._highlightedHubIndex();
    const next = current < 0
      ? 0
      : (current + step + hubs.length) % hubs.length;

    this._highlightedHubIndex.set(next);
  }

  private syncHighlightedHubIndex(): void {
    const hubs = this.filteredHubs;
    if (hubs.length === 0) {
      this._highlightedHubIndex.set(-1);
      return;
    }

    const selectedHubId = this._selectedHubId();
    if (selectedHubId) {
      const selectedIndex = hubs.findIndex(h => h.hubId === selectedHubId);
      if (selectedIndex >= 0) {
        this._highlightedHubIndex.set(selectedIndex);
        return;
      }
    }

    this._highlightedHubIndex.set(0);
  }

  private getSelectedHubDisplayText(): string {
    const selectedHub = this._hubs().find(h => h.hubId === this._selectedHubId());
    return selectedHub ? this.getHubDisplayText(selectedHub) : '';
  }

  getHubDisplayText(hub: HubResponse): string {
    return `${hub.name}${hub.address ? ` - ${hub.address}` : ''}`;
  }

  // ==== STATUS BADGE ====

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Draft': 'status-draft',
      'Booked': 'status-booked',
      'PickedUp': 'status-pickedup',
      'InTransit': 'status-intransit',
      'OutForDelivery': 'status-outfordelivery',
      'Delivered': 'status-delivered'
    };

    return map[this.normalizeStatusValue(status)] ?? 'status-draft';
  }

  getEventBadgeClass(status: string): string {
    const normalized = String(status ?? '').toLowerCase().replace(/\s+/g, '');
    if (normalized.includes('draft') || normalized.includes('created')) return 'event-created';
    if (normalized.includes('booked')) return 'event-booked';
    if (normalized.includes('pickedup') || normalized.includes('pickup')) return 'event-pickup';
    if (normalized.includes('intransit')) return 'event-transit';
    if (normalized.includes('outfordelivery')) return 'event-out';
    if (normalized.includes('delivered')) return 'event-delivered';
    return 'event-default';
  }

  normalizeStatus(status: string): string {
    const normalized = this.normalizeStatusValue(status);
    if (!normalized) {
      return 'Unknown';
    }

    return normalized
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .trim();
  }

  getTimelineLocation(event: TrackingEvent): string {
    const shipment = this._shipment();
    const status = this.normalizeStatusValue(event?.status);

    if (status === 'PickedUp') {
      return this.formatAddressSingleLine(shipment?.senderAddress) || event.location || 'Location not available';
    }

    if (status === 'Delivered') {
      return this.formatAddressSingleLine(shipment?.receiverAddress) || event.location || 'Location not available';
    }

    return event.location || 'Location not available';
  }

  private normalizeStatusValue(status: unknown): string {
    const raw = String(status ?? '').trim();
    if (!raw) {
      return '';
    }

    const collapsed = raw.toLowerCase().replace(/[^a-z]/g, '');
    if (collapsed === 'draft') return 'Draft';
    if (collapsed === 'booked') return 'Booked';
    if (collapsed === 'pickedup') return 'PickedUp';
    if (collapsed === 'intransit') return 'InTransit';
    if (collapsed === 'outfordelivery') return 'OutForDelivery';
    if (collapsed === 'delivered') return 'Delivered';

    return raw;
  }

  private ensureTimelineIncludesCurrentStatus(events: TrackingEvent[], trackingNumber: string): TrackingEvent[] {
    const shipmentStatus = this.currentShipmentStatus;
    const currentRank = this.getStatusRank(shipmentStatus);
    if (currentRank <= 0) {
      return events;
    }

    const hasCurrent = events.some(e => this.normalizeStatusValue(e.status) === shipmentStatus);
    if (hasCurrent) {
      return events;
    }

    const latestTimestamp = events.length > 0
      ? new Date(events[0].timestamp)
      : new Date();
    const injectedTimestamp = Number.isNaN(latestTimestamp.getTime())
      ? new Date().toISOString()
      : new Date(latestTimestamp.getTime() + 60_000).toISOString();

    const injectedEvent: TrackingEvent = {
      trackingNumber,
      status: shipmentStatus,
      description: this.getDefaultDescription(shipmentStatus),
      location: this.getDefaultLocationByStatus(shipmentStatus),
      timestamp: injectedTimestamp
    };

    return [injectedEvent, ...events]
      .sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime());
  }

  private buildSyntheticTimeline(trackingNumber: string): TrackingEvent[] {
    const status = this.currentShipmentStatus;
    if (this.getStatusRank(status) <= 0) {
      return [];
    }

    return [
      {
        trackingNumber,
        status,
        description: this.getDefaultDescription(status),
        location: this.getDefaultLocationByStatus(status),
        timestamp: new Date().toISOString()
      }
    ];
  }

  private getStatusRank(status: string): number {
    const normalized = this.normalizeStatusValue(status);
    if (normalized === 'Draft') return 1;
    if (normalized === 'Booked') return 2;
    if (normalized === 'PickedUp') return 3;
    if (normalized === 'InTransit') return 4;
    if (normalized === 'OutForDelivery') return 5;
    if (normalized === 'Delivered') return 6;
    return 0;
  }

  private getDefaultDescription(status: string): string {
    const normalized = this.normalizeStatusValue(status);
    if (normalized === 'Delivered') return 'Shipment delivered to receiver';
    if (normalized === 'OutForDelivery') return 'Shipment is out for delivery';
    if (normalized === 'InTransit') return 'Shipment moved through transit hub';
    if (normalized === 'PickedUp') return 'Shipment picked up from sender';
    if (normalized === 'Booked') return 'Shipment booked by admin';
    return 'Shipment status updated';
  }

  private getActionLabel(action: 'book' | 'pickup' | 'transit' | 'outfordelivery' | 'deliver'): string {
    if (action === 'book') return 'Booked';
    if (action === 'pickup') return 'Picked Up';
    if (action === 'transit') return 'In Transit';
    if (action === 'outfordelivery') return 'Out for Delivery';
    return 'Delivered';
  }

  private getActionStatus(action: 'book' | 'pickup' | 'transit' | 'outfordelivery' | 'deliver'): ShipmentResponse['status'] {
    if (action === 'book') return 'Booked';
    if (action === 'pickup') return 'PickedUp';
    if (action === 'transit') return 'InTransit';
    if (action === 'outfordelivery') return 'OutForDelivery';
    return 'Delivered';
  }

  private getDefaultLocationByStatus(status: string): string {
    const shipment = this._shipment();
    const normalized = this.normalizeStatusValue(status);
    if (normalized === 'PickedUp') {
      return this.formatAddressSingleLine(shipment?.senderAddress) || 'Location not available';
    }
    if (normalized === 'Delivered') {
      return this.formatAddressSingleLine(shipment?.receiverAddress) || 'Location not available';
    }
    return this.getSelectedHubDisplayText() || 'Location not available';
  }

  private formatAddressSingleLine(address: {
    street?: string;
    city?: string;
    state?: string;
    country?: string;
  } | null | undefined): string {
    if (!address) {
      return '';
    }

    return [address.street, address.city, address.state, address.country]
      .map(part => String(part ?? '').trim())
      .filter(Boolean)
      .join(', ');
  }

  deleteShipment(): void {
    if (!confirm('Warning: Are you strictly sure you want to delete this entire shipment? This action cannot be undone.')) return;
    
    this.shipmentService.delete(this.shipmentId).subscribe({
      next: () => {
        this.router.navigate(['/admin/shipments']);
      },
      error: () => {
        this._error.set('Failed to delete shipment.');
      }
    });
  }

  // ==== INVOICE ACTIONS ====

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this._showInvoiceMenu()) {
      return;
    }

    const target = event.target as Node | null;
    const actionsElement = this.invoiceActions?.nativeElement as HTMLElement | undefined;

    if (actionsElement && target && !actionsElement.contains(target)) {
      this._showInvoiceMenu.set(false);
    }
  }

  toggleInvoiceMenu(): void {
    this._showInvoiceMenu.set(!this._showInvoiceMenu());
  }

  printInvoice(): void {
    const shipment = this._shipment();
    if (!shipment) {
      this._error.set('Shipment data not available.');
      return;
    }

    try {
      const element = this.invoiceContent?.nativeElement;
      if (!element) {
        this._error.set('Cannot print invoice.');
        return;
      }

      const printWindow = window.open('', '', 'width=800,height=600');
      if (!printWindow) {
        this._error.set('Pop-up blocked. Please allow pop-ups and try again.');
        return;
      }

      const styles = `
        <style>
          body { font-family: Arial, sans-serif; color: #1e293b; }
          .invoice-doc { border: 1px solid #e2e8f0; border-radius: 12px; padding: 12px; background: #fff; }
          .invoice-doc-header { display: flex; justify-content: space-between; align-items: flex-start; gap: 12px; border-bottom: 1px solid #e2e8f0; padding-bottom: 10px; margin-bottom: 10px; }
          .invoice-company-name { font-size: 16px; font-weight: 700; color: #0f172a; }
          .invoice-company-sub { font-size: 12px; color: #64748b; margin-top: 2px; }
          .invoice-doc-title-wrap { text-align: right; }
          .invoice-doc-title { font-size: 16px; font-weight: 700; color: #1d4ed8; }
          .invoice-doc-subtitle { font-size: 11px; color: #64748b; margin-top: 2px; }
          .invoice-meta-grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 8px; margin-bottom: 10px; }
          .invoice-meta-item { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 8px; }
          .invoice-col-label { font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.04em; color: #64748b; margin-bottom: 4px; }
          .invoice-col-value { font-size: 13px; color: #1e293b; }
          .invoice-party-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; margin-bottom: 12px; }
          .invoice-party-card { border: 1px solid #e2e8f0; border-radius: 10px; padding: 10px; }
          .invoice-label { font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.04em; color: #94a3b8; margin-bottom: 5px; }
          .invoice-name { font-size: 13px; font-weight: 700; color: #1e293b; margin-bottom: 4px; }
          .invoice-phone,.invoice-address { font-size: 12px; color: #475569; line-height: 1.45; }
          .invoice-table-wrap { border: 1px solid #e2e8f0; border-radius: 10px; overflow: hidden; margin-bottom: 12px; }
          .invoice-table { width: 100%; border-collapse: collapse; }
          .invoice-table th,.invoice-table td { padding: 8px 10px; border-bottom: 1px solid #e2e8f0; text-align: left; font-size: 12px; color: #1e293b; }
          .invoice-table th { background: #f8fafc; font-size: 11px; text-transform: uppercase; letter-spacing: 0.04em; color: #64748b; }
          .invoice-table tbody tr:last-child td { border-bottom: none; }
          .invoice-summary-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 8px; margin-bottom: 10px; }
          .invoice-summary-item { border: 1px solid #e2e8f0; border-radius: 8px; padding: 8px; display: flex; justify-content: space-between; align-items: center; background: #fff; font-size: 12px; color: #475569; }
          .invoice-summary-item strong { color: #0f172a; font-size: 13px; }
          .invoice-summary-item.total-amount { background: #eff6ff; border-color: #bfdbfe; }
          .invoice-summary-item.total-amount strong { color: #1d4ed8; }
          .invoice-note { font-size: 11px; color: #64748b; border-top: 1px dashed #cbd5e1; padding-top: 7px; }
          @media print { body { margin: 0; padding: 10mm; } }
        </style>
      `;

      printWindow.document.write('<!DOCTYPE html><html><head>' + styles + '</head><body>');
      printWindow.document.write(element.innerHTML);
      printWindow.document.write('</body></html>');
      printWindow.document.close();

      setTimeout(() => {
        printWindow.print();
        printWindow.close();
      }, 250);

      this._showInvoiceMenu.set(false);
    } catch (err) {
      this._error.set('Error printing invoice.');
    }
  }

}

