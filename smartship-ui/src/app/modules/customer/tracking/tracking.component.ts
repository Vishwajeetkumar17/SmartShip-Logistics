import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { TrackingService } from '../../../core/services/tracking.service';
import { ShipmentService } from '../../../core/services/shipment.service';
import { DocumentService } from '../../../core/services/document.service';
import { TrackingResponse } from '../../../shared/models/tracking.model';
import { ShipmentResponse } from '../../../shared/models/shipment.model';
import { DocumentResponse } from '../../../shared/models/document.model';

@Component({
  selector: 'app-tracking',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './tracking.component.html',
  styleUrl: './tracking.component.css'
})
/**
 * Customer tracking screen.
 * Resolves shipment status/location from multiple sources (tracking timeline + shipment data),
 * supports document uploads/downloads, and allows raising shipment issues via the gateway.
 */
export class TrackingComponent implements OnInit {
  private trackingService = inject(TrackingService);
  private shipmentService = inject(ShipmentService);
  private documentService = inject(DocumentService);
  private route = inject(ActivatedRoute);

  private readonly _trackingData = signal<TrackingResponse | null>(null);
  private readonly _shipmentData = signal<ShipmentResponse | null>(null);
  private readonly _documents = signal<DocumentResponse[]>([]);
  private readonly _isLoading = signal(false);
  private readonly _searched = signal(false);
  private readonly _errorMessage = signal('');
  private readonly _uploadError = signal('');
  private readonly _uploading = signal(false);
  private readonly _isDragOver = signal(false);
  private readonly _deleting = signal(false);
  private readonly _successMessage = signal('');
  private readonly _issuePanelOpen = signal(false);
  private readonly _issueSubmitting = signal(false);
  private readonly _issueType = signal('Delay');
  private readonly _issueDescription = signal('');
  private readonly _issueError = signal('');
  private readonly _issueTypeDropdownOpen = signal(false);

  trackingNumber = '';

  ngOnInit(): void {
    const num = this.route.snapshot.queryParamMap.get('track');
    if (num) {
      this.trackingNumber = num;
      this.onTrack();
    }
  }

  get trackingData(): TrackingResponse | null { return this._trackingData(); }
  get shipmentData(): ShipmentResponse | null { return this._shipmentData(); }
  get documents(): DocumentResponse[] { return this._documents(); }
  get isLoading(): boolean { return this._isLoading(); }
  get searched(): boolean { return this._searched(); }
  get errorMessage(): string { return this._errorMessage(); }
  get uploadError(): string { return this._uploadError(); }
  get uploading(): boolean { return this._uploading(); }
  get isDragOver(): boolean { return this._isDragOver(); }
  get deleting(): boolean { return this._deleting(); }
  get successMessage(): string { return this._successMessage(); }
  get issuePanelOpen(): boolean { return this._issuePanelOpen(); }
  get issueSubmitting(): boolean { return this._issueSubmitting(); }
  get issueType(): string { return this._issueType(); }
  get issueDescription(): string { return this._issueDescription(); }
  get issueError(): string { return this._issueError(); }
  get issueTypeDropdownOpen(): boolean { return this._issueTypeDropdownOpen(); }

  get canDeleteShipment(): boolean {
    const shipment = this._shipmentData();
    if (!shipment) return false;

    return String(shipment.status ?? '').toLowerCase() === 'draft';
  }

  get serviceType(): string {
    const type = String(this._shipmentData()?.serviceType ?? '').trim();
    return type || 'Standard';
  }

  get estimatedDeliveryDate(): string {
    const shipment = this._shipmentData();
    if (!shipment) return 'N/A';

    // If explicit pickup schedule date exists, use it
    if (shipment.pickupSchedule?.pickupDate) {
      return new Date(shipment.pickupSchedule.pickupDate).toLocaleDateString('en-IN', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
      });
    }

    // Calculate based on shipment status
    const timeline = this._trackingData()?.timeline ?? [];
    const createdDate = shipment.createdAt ? new Date(shipment.createdAt) : new Date();
    
    // Determine days until delivery based on status
    let estimatedDaysRemaining = 3;
    const status = String(shipment.status ?? '').toLowerCase();
    
    if (status.includes('delivered')) {
      const deliveredEvent = timeline.find(e => String(e.status ?? '').toLowerCase().includes('delivered'));
      if (deliveredEvent?.timestamp) {
        return new Date(deliveredEvent.timestamp).toLocaleDateString('en-IN', {
          year: 'numeric',
          month: 'short',
          day: 'numeric'
        });
      }
      const latestEvent = timeline[timeline.length - 1];
      if (latestEvent?.timestamp) {
        return new Date(latestEvent.timestamp).toLocaleDateString('en-IN', {
          year: 'numeric',
          month: 'short',
          day: 'numeric'
        });
      }
      return 'Delivered';
    }
    
    if (status.includes('outfordelivery')) {
      estimatedDaysRemaining = 1;
    } else if (status.includes('intransit') || status.includes('transit')) {
      estimatedDaysRemaining = 2;
    } else if (status.includes('pickedup') || status.includes('pickup')) {
      estimatedDaysRemaining = 3;
    } else if (status.includes('booked')) {
      estimatedDaysRemaining = 4;
    } else if (status.includes('draft')) {
      estimatedDaysRemaining = 5;
    }

    const estimatedDate = new Date(createdDate);
    estimatedDate.setDate(estimatedDate.getDate() + estimatedDaysRemaining);
    return estimatedDate.toLocaleDateString('en-IN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }

  get statusClass(): string {
    const s = this.trackingData?.currentStatus?.toLowerCase() ?? '';
    if (s.includes('delivered')) return 'status-delivered';
    if (s.includes('transit') || s.includes('in transit') || s.includes('intransit')) return 'status-intransit';
    if (s.includes('booked')) return 'status-booked';
    if (s.includes('picked up') || s.includes('pickedup') || s.includes('pickup')) return 'status-pickedup';
    if (s.includes('out for delivery') || s.includes('outfordelivery')) return 'status-outfordelivery';
    if (s.includes('failed')) return 'status-failed';
    if (s.includes('cancelled')) return 'status-cancelled';
    if (s.includes('created') || s.includes('draft') || s.includes('pending')) return 'status-draft';
    return 'status-draft';
  }

  getEventBadgeClass(status: string): string {
    const s = status?.toLowerCase() ?? '';
    if (s.includes('created') || s.includes('draft')) return 'event-created';
    if (s.includes('booked')) return 'event-booked';
    if (s.includes('pickedup') || s.includes('picked')) return 'event-pickup';
    if (s.includes('transit')) return 'event-transit';
    if (s.includes('outfordelivery') || s.includes('out for') || s.includes('out_for')) return 'event-out';
    if (s.includes('delivered')) return 'event-delivered';
    return 'event-default';
  }

  onTrack(): void {
    if (!this.trackingNumber.trim()) return;
    this._isLoading.set(true);
    this._searched.set(true);
    this._errorMessage.set('');
    this._successMessage.set('');
    this._trackingData.set(null);
    this._shipmentData.set(null);
    this._documents.set([]);
    this._issuePanelOpen.set(false);
    this._issueDescription.set('');
    this._issueError.set('');

    const tn = this.trackingNumber.trim();

    forkJoin({
      tracking: this.trackingService.getTrackingInfo(tn),
      timeline: this.trackingService.getTimeline(tn).pipe(catchError(() => of([]))),
      shipment: this.shipmentService.getByTrackingNumber(tn).pipe(catchError(() => of(null)))
    }).subscribe({
      next: ({ tracking, timeline, shipment }) => {
        const normalizedTimeline = this.normalizeTimeline([
          ...(tracking?.timeline ?? []),
          ...(Array.isArray(timeline) ? timeline : [])
        ]);

        const latestEvent = normalizedTimeline[normalizedTimeline.length - 1];
        const timelineStatus = this.humanizeStatus(latestEvent?.status || tracking.currentStatus);
        const shipmentStatus = this.humanizeStatus(shipment?.status);

        const resolvedStatus = this.statusRank(shipmentStatus) > this.statusRank(timelineStatus)
          ? shipmentStatus
          : timelineStatus;

        const flowTimeline = this.buildFlowTimeline({
          trackingNumber: tn,
          resolvedStatus,
          normalizedTimeline,
          shipment,
          initialTimestamp: tracking.initialEventTimestamp,
          latestTimestamp: tracking.latestEventTimestamp,
          fallbackLocation: tracking.currentLocation
        });

        const baseTimeline = normalizedTimeline.length > 0
          ? normalizedTimeline
          : flowTimeline;

        const historyTimeline = this.ensureTimelineIncludesCurrentStatus(
          baseTimeline,
          resolvedStatus,
          tn,
          shipment,
          tracking.currentLocation
        );

        const latestResolvedEvent = historyTimeline[historyTimeline.length - 1];
        const resolvedLocation = this.resolveCurrentLocationForStatus(
          resolvedStatus,
          shipment,
          latestResolvedEvent?.location,
          tracking.currentLocation
        );

        this._trackingData.set({
          ...tracking,
          currentStatus: resolvedStatus,
          currentLocation: resolvedLocation,
          timeline: historyTimeline.length > 0 ? historyTimeline : (tracking.timeline ?? [])
        });

        if (shipment) {
          this._shipmentData.set(shipment);
          this.documentService.getByShipment(shipment.shipmentId).subscribe({
            next: (docs) => { this._documents.set(docs); this._isLoading.set(false); },
            error: () => { this._isLoading.set(false); }
          });
        } else {
          this._isLoading.set(false);
        }
      },
      error: (err) => {
        this._isLoading.set(false);
        this._errorMessage.set(err.error?.message || 'Tracking number not found.');
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.[0]) this.uploadFile(input.files[0]);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this._isDragOver.set(true);
  }

  onDragLeave(): void {
    this._isDragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this._isDragOver.set(false);
    const file = event.dataTransfer?.files[0];
    if (file) this.uploadFile(file);
  }

  onDeleteShipment(): void {
    const shipment = this._shipmentData();
    if (!shipment || !this.canDeleteShipment || this._deleting()) {
      return;
    }

    const confirmed = window.confirm(`Delete shipment ${shipment.trackingNumber}? This action cannot be undone.`);
    if (!confirmed) {
      return;
    }

    this._deleting.set(true);
    this._errorMessage.set('');
    this._successMessage.set('');

    this.shipmentService.deleteMyShipment(shipment.shipmentId).subscribe({
      next: () => {
        this._deleting.set(false);
        this._trackingData.set(null);
        this._shipmentData.set(null);
        this._documents.set([]);
        this._searched.set(true);
        this._successMessage.set('Shipment deleted successfully.');
      },
      error: (err) => {
        this._deleting.set(false);
        this._errorMessage.set(err.error?.detail || err.error?.message || 'Failed to delete shipment.');
      }
    });
  }

  openIssuePanel(): void {
    this._issuePanelOpen.set(true);
    this._issueError.set('');
    this._successMessage.set('');
    this._issueType.set('Delay');
    this._issueDescription.set('');
  }

  closeIssuePanel(): void {
    this._issuePanelOpen.set(false);
    this._issueError.set('');
    this._issueDescription.set('');
  }

  setIssueType(value: string): void {
    this._issueType.set(value);
    this._issueError.set('');
    this._issueTypeDropdownOpen.set(false);
  }

  toggleIssueTypeDropdown(): void {
    this._issueTypeDropdownOpen.update(v => !v);
  }

  setIssueDescription(value: string): void {
    this._issueDescription.set(value);
    this._issueError.set('');
  }

  submitIssue(): void {
    const shipment = this._shipmentData();
    if (!shipment) {
      this._issueError.set('Shipment details are not available.');
      return;
    }

    const description = this._issueDescription().trim();
    if (!description) {
      this._issueError.set('Please enter issue details.');
      return;
    }

    this._issueSubmitting.set(true);
    this._issueError.set('');
    this._errorMessage.set('');
    this._successMessage.set('');

    this.shipmentService.raiseIssue(shipment.shipmentId, {
      issueType: this._issueType(),
      description
    }).pipe(
      finalize(() => this._issueSubmitting.set(false))
    ).subscribe({
      next: (response) => {
        this._successMessage.set(response?.message || 'Issue submitted successfully. Our admin team will review it.');
        this.closeIssuePanel();
      },
      error: (err) => {
        const backendMessage = err?.error?.message || err?.error?.detail;
        if (backendMessage) {
          this._issueError.set(backendMessage);
          return;
        }

        if (err?.status === 403) {
          this._issueError.set('You can raise issue only for your own shipment.');
          return;
        }

        if (err?.status === 401) {
          this._issueError.set('Your session expired. Please login again.');
          return;
        }

        this._issueError.set('Failed to submit issue. Please try again.');
      }
    });
  }

  private uploadFile(file: File): void {
    const shipment = this._shipmentData();
    if (!shipment) {
      this._uploadError.set('Cannot upload: shipment information not available.');
      return;
    }
    const allowed = ['application/pdf', 'image/png', 'image/jpeg'];
    if (!allowed.includes(file.type)) {
      this._uploadError.set('Only PDF, PNG, and JPG files are allowed.');
      return;
    }
    if (file.size > 10 * 1024 * 1024) {
      this._uploadError.set('File size must be less than 10MB.');
      return;
    }
    this._uploadError.set('');
    this._uploading.set(true);
    this.documentService.upload(shipment.shipmentId, file).subscribe({
      next: (doc) => {
        this._documents.update(docs => [...docs, doc]);
        this._uploading.set(false);
      },
      error: (err) => {
        this._uploadError.set(err.error?.message || 'Upload failed. Please try again.');
        this._uploading.set(false);
      }
    });
  }

  downloadDocument(url: string, fileName: string): void {
    this._uploadError.set('');

    const sourceUrl = String(url ?? '').trim();
    if (!sourceUrl) {
      this._uploadError.set('Document URL is missing.');
      return;
    }

    if (sourceUrl.startsWith('events://')) {
      const eventPayload = {
        fileName,
        source: sourceUrl,
        generatedAt: new Date().toISOString(),
        note: 'This is an event-generated document reference.'
      };
      const blob = new Blob([JSON.stringify(eventPayload, null, 2)], { type: 'application/json' });
      this.triggerBlobDownload(blob, fileName.endsWith('.json') ? fileName : `${fileName}.json`);
      return;
    }

    this.documentService.downloadFile(sourceUrl).subscribe({
      next: (blob) => {
        this.triggerBlobDownload(blob, fileName);
      },
      error: () => {
        this._uploadError.set('Unable to download document. Please try again.');
      }
    });
  }

  downloadInvoice(): void {
    const shipment = this._shipmentData();
    const tracking = this._trackingData();

    if (!shipment || !tracking) {
      this._errorMessage.set('Shipment details are not available for invoice print.');
      return;
    }

    const packageRows = (shipment.packages ?? []).length > 0
      ? (shipment.packages ?? []).map((pkg, index) => `
          <tr>
            <td>${index + 1}</td>
            <td>${this.escapeHtml(pkg.description || 'Package')}</td>
            <td>${this.formatNumber(pkg.length)} × ${this.formatNumber(pkg.width)} × ${this.formatNumber(pkg.height)}</td>
            <td>${this.formatNumber(pkg.weight)}</td>
          </tr>
        `).join('')
      : `
          <tr>
            <td>1</td>
            <td>Shipment package</td>
            <td>—</td>
            <td>${this.formatNumber(shipment.totalWeight ?? 0)}</td>
          </tr>
        `;

    const estimatedCost = (shipment.estimatedCost ?? 0) > 0
      ? `₹${Number(shipment.estimatedCost).toFixed(2)}`
      : '₹0.00';

    const html = `<!doctype html>
<html>
  <head>
    <meta charset="utf-8" />
    <title>Invoice-${this.escapeHtml(shipment.trackingNumber)}</title>
    <style>
      body { font-family: Arial, sans-serif; color: #1f2937; margin: 0; padding: 24px; }
      .invoice { max-width: 900px; margin: 0 auto; border: 1px solid #e5e7eb; border-radius: 12px; padding: 18px; }
      .header { display: flex; justify-content: space-between; border-bottom: 1px solid #e5e7eb; padding-bottom: 12px; margin-bottom: 12px; }
      .company { font-weight: 700; font-size: 18px; color: #0f172a; }
      .subtitle { font-size: 12px; color: #64748b; margin-top: 2px; }
      .title { text-align: right; }
      .title h2 { margin: 0; color: #1d4ed8; font-size: 20px; }
      .meta { display: grid; grid-template-columns: repeat(4, minmax(0,1fr)); gap: 8px; margin-bottom: 12px; }
      .meta-item { border: 1px solid #e5e7eb; border-radius: 8px; padding: 8px; background: #f8fafc; }
      .label { font-size: 11px; color: #64748b; text-transform: uppercase; letter-spacing: .04em; }
      .value { font-size: 13px; color: #0f172a; margin-top: 4px; }
      .parties { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; margin-bottom: 12px; }
      .party { border: 1px solid #e5e7eb; border-radius: 10px; padding: 10px; }
      .party h3 { margin: 0 0 6px; font-size: 12px; color: #64748b; text-transform: uppercase; }
      .party .name { font-weight: 700; font-size: 14px; color: #0f172a; margin-bottom: 4px; }
      .party .line { font-size: 12px; color: #334155; line-height: 1.45; }
      table { width: 100%; border-collapse: collapse; margin-bottom: 12px; }
      th, td { border: 1px solid #e5e7eb; padding: 8px; text-align: left; font-size: 12px; }
      th { background: #f8fafc; color: #64748b; text-transform: uppercase; letter-spacing: .04em; }
      .summary { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 8px; }
      .sum-item { border: 1px solid #e5e7eb; border-radius: 8px; padding: 8px; display:flex; justify-content:space-between; }
      .sum-item.total { background: #eff6ff; border-color: #bfdbfe; }
      .note { margin-top: 12px; font-size: 11px; color: #64748b; border-top: 1px dashed #cbd5e1; padding-top: 8px; }
    </style>
  </head>
  <body>
    <div class="invoice">
      <div class="header">
        <div>
          <div class="company">SmartShip Logistics Pvt. Ltd.</div>
          <div class="subtitle">Reliable Domestic & Express Shipping Services</div>
        </div>
        <div class="title">
          <h2>Shipment Invoice</h2>
          <div class="subtitle">Tax Invoice / Shipping Invoice</div>
        </div>
      </div>

      <div class="meta">
        <div class="meta-item"><div class="label">Invoice No</div><div class="value">INV-SH${String(shipment.shipmentId).padStart(6, '0')}</div></div>
        <div class="meta-item"><div class="label">Invoice Date</div><div class="value">${new Date(shipment.createdAt).toLocaleDateString('en-IN')}</div></div>
        <div class="meta-item"><div class="label">Tracking No</div><div class="value">${this.escapeHtml(shipment.trackingNumber)}</div></div>
        <div class="meta-item"><div class="label">Current Status</div><div class="value">${this.escapeHtml(tracking.currentStatus)}</div></div>
      </div>

      <div class="parties">
        <div class="party">
          <h3>Bill From (Sender)</h3>
          <div class="name">${this.escapeHtml(shipment.senderName || 'Name not provided')}</div>
          <div class="line">${this.escapeHtml(shipment.senderAddress?.street || '—')}</div>
          <div class="line">${this.escapeHtml(shipment.senderAddress?.city || '')}, ${this.escapeHtml(shipment.senderAddress?.state || '')}</div>
          <div class="line">${this.escapeHtml(shipment.senderAddress?.postalCode || '')} ${this.escapeHtml(shipment.senderAddress?.country || '')}</div>
        </div>
        <div class="party">
          <h3>Bill To (Receiver)</h3>
          <div class="name">${this.escapeHtml(shipment.receiverName || 'Name not provided')}</div>
          <div class="line">${this.escapeHtml(shipment.receiverAddress?.street || '—')}</div>
          <div class="line">${this.escapeHtml(shipment.receiverAddress?.city || '')}, ${this.escapeHtml(shipment.receiverAddress?.state || '')}</div>
          <div class="line">${this.escapeHtml(shipment.receiverAddress?.postalCode || '')} ${this.escapeHtml(shipment.receiverAddress?.country || '')}</div>
        </div>
      </div>

      <table>
        <thead>
          <tr>
            <th>#</th>
            <th>Package Description</th>
            <th>Dimensions (cm)</th>
            <th>Weight (kg)</th>
          </tr>
        </thead>
        <tbody>${packageRows}</tbody>
      </table>

      <div class="summary">
        <div class="sum-item"><span>Total Packages</span><strong>${(shipment.packages ?? []).length || 1}</strong></div>
        <div class="sum-item"><span>Total Weight</span><strong>${this.formatNumber(shipment.totalWeight ?? 0)} kg</strong></div>
        <div class="sum-item total"><span>Estimated Amount</span><strong>${estimatedCost}</strong></div>
      </div>

      <div class="note">This is a system-generated shipment invoice for logistics and delivery reference.</div>
    </div>
  </body>
</html>`;

    const printWindow = window.open('', '', 'width=900,height=700');
    if (!printWindow) {
      this._errorMessage.set('Pop-up blocked. Please allow pop-ups to print invoice.');
      return;
    }

    printWindow.document.open();
    printWindow.document.write(html);
    printWindow.document.close();

    setTimeout(() => {
      printWindow.focus();
      printWindow.print();
      printWindow.close();
    }, 250);
  }

  private triggerBlobDownload(blob: Blob, fileName: string): void {
    const objectUrl = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = objectUrl;
    anchor.download = fileName;
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
    URL.revokeObjectURL(objectUrl);
  }

  private escapeHtml(value: string): string {
    return String(value ?? '')
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }

  private formatNumber(value: unknown): string {
    const numeric = Number(value ?? 0);
    return Number.isFinite(numeric) ? numeric.toString() : '0';
  }

  getDotClass(index: number, total: number): string {
    if (index === total - 1) return 'dot-blue';
    return 'dot-green';
  }

  getTimelineLocation(event: any): string {
    const shipment = this._shipmentData();
    const status = this.humanizeStatus(event?.status).toLowerCase().replace(/\s+/g, '');

    if (status.includes('pickedup') || status.includes('pickup')) {
      return this.formatAddressSingleLine(shipment?.senderAddress) || event?.location || 'Location not available';
    }

    if (status.includes('delivered')) {
      return this.formatAddressSingleLine(shipment?.receiverAddress) || event?.location || 'Location not available';
    }

    return event?.location || 'Location not available';
  }

  private normalizeTimeline(events: any[]): any[] {
    const rows = (Array.isArray(events) ? events : [])
      .filter((event): event is any => !!event && typeof event === 'object')
      .map((event) => ({
        ...event,
        status: this.humanizeStatus(event.status),
        timestamp: event.timestamp || event.Timestamp || ''
      }));

    const unique = new Map<string, any>();
    for (const event of rows) {
      const key = `${event.eventId ?? ''}|${event.status ?? ''}|${event.location ?? ''}|${event.timestamp ?? ''}`;
      if (!unique.has(key)) {
        unique.set(key, event);
      }
    }

    return Array.from(unique.values())
      .sort((a, b) => this.toTime(b.timestamp) - this.toTime(a.timestamp));
  }

  private toTime(value: string): number {
    const dt = new Date(value).getTime();
    return Number.isNaN(dt) ? 0 : dt;
  }

  private humanizeStatus(status: unknown): string {
    const raw = String(status ?? '').trim();
    if (!raw) return 'Unknown';

    const normalized = raw
      .replace(/_/g, ' ')
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .toLowerCase();

    return normalized
      .split(' ')
      .filter(Boolean)
      .map(part => part.charAt(0).toUpperCase() + part.slice(1))
      .join(' ');
  }

  private statusRank(status: string): number {
    const normalized = (status || '').toLowerCase().replace(/\s+/g, '');
    if (normalized.includes('draft') || normalized.includes('created')) return 1;
    if (normalized.includes('booked')) return 2;
    if (normalized.includes('pickedup') || normalized.includes('pickup')) return 3;
    if (normalized.includes('intransit')) return 4;
    if (normalized.includes('outfordelivery')) return 5;
    if (normalized.includes('delivered')) return 6;
    return 0;
  }

  private defaultStatusDescription(status: string): string {
    const normalized = (status || '').toLowerCase().replace(/\s+/g, '');
    if (normalized.includes('delivered')) return 'Shipment delivered successfully';
    if (normalized.includes('outfordelivery')) return 'Shipment is out for delivery';
    if (normalized.includes('intransit')) return 'Shipment is in transit';
    if (normalized.includes('pickedup') || normalized.includes('pickup')) return 'Shipment picked up from sender';
    if (normalized.includes('booked')) return 'Shipment request created and confirmed';
    return 'Shipment status updated';
  }

  private ensureTimelineIncludesCurrentStatus(
    timeline: any[],
    resolvedStatus: string,
    trackingNumber: string,
    shipment: ShipmentResponse | null,
    fallbackLocation?: string
  ): any[] {
    const currentRank = this.statusRank(resolvedStatus);
    if (currentRank <= 0) {
      return timeline;
    }

    const hasCurrent = timeline.some(event => this.statusRank(event?.status) === currentRank);
    if (hasCurrent) {
      return timeline;
    }

    const latest = timeline[timeline.length - 1];
    const baseTime = this.toTime(latest?.timestamp || '');
    const injectedTime = baseTime > 0
      ? new Date(baseTime + 60_000).toISOString()
      : new Date().toISOString();

    const injected = {
      eventId: undefined,
      trackingNumber,
      status: this.humanizeStatus(resolvedStatus),
      location: this.getDefaultLocationForStatus(resolvedStatus, shipment, fallbackLocation),
      description: this.defaultStatusDescription(resolvedStatus),
      timestamp: injectedTime
    };

    return [...timeline, injected].sort((a, b) => this.toTime(a.timestamp) - this.toTime(b.timestamp));
  }

  private getDefaultLocationForStatus(
    status: string,
    shipment: ShipmentResponse | null,
    fallbackLocation?: string
  ): string {
    const normalized = this.humanizeStatus(status).toLowerCase().replace(/\s+/g, '');
    if (normalized.includes('pickedup') || normalized.includes('pickup')) {
      return this.formatAddressSingleLine(shipment?.senderAddress) || fallbackLocation || 'Location not available';
    }
    if (normalized.includes('delivered')) {
      return this.formatAddressSingleLine(shipment?.receiverAddress) || fallbackLocation || 'Location not available';
    }
    return fallbackLocation || 'Location not available';
  }

  private resolveCurrentLocationForStatus(
    status: string,
    shipment: ShipmentResponse | null,
    timelineLocation?: string,
    trackingLocation?: string
  ): string {
    const normalized = this.humanizeStatus(status).toLowerCase().replace(/\s+/g, '');

    if (normalized.includes('delivered')) {
      return this.formatAddressSingleLine(shipment?.receiverAddress)
        || timelineLocation
        || trackingLocation
        || 'N/A';
    }

    return timelineLocation || trackingLocation || 'N/A';
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

  private buildFlowTimeline(params: {
    trackingNumber: string;
    resolvedStatus: string;
    normalizedTimeline: any[];
    shipment: ShipmentResponse | null;
    initialTimestamp?: string;
    latestTimestamp?: string;
    fallbackLocation?: string;
  }): any[] {
    const flow = ['Booked', 'Picked Up', 'In Transit', 'Out For Delivery', 'Delivered'];
    const currentRank = this.statusRank(params.resolvedStatus);
    const maxFlowIndex = currentRank > 0 ? Math.min(currentRank, flow.length) : 1;

    const stageEvents = flow.map((stage) => {
      const rank = this.statusRank(stage);
      const matched = params.normalizedTimeline
        .filter((event) => this.statusRank(event.status) === rank)
        .sort((a, b) => this.toTime(a.timestamp) - this.toTime(b.timestamp));
      return matched[matched.length - 1] ?? null;
    });

    const seedTime = this.pickSeedTime([
      params.shipment?.createdAt,
      params.initialTimestamp,
      params.normalizedTimeline[0]?.timestamp,
      new Date().toISOString()
    ]);

    const result: any[] = [];
    let lastTime = seedTime;

    for (let index = 0; index < maxFlowIndex; index++) {
      const stage = flow[index];
      const existing = stageEvents[index];
      const rawTime = existing?.timestamp;
      const stageTime = this.pickSeedTime([rawTime, lastTime.toISOString()]);
      if (stageTime.getTime() < lastTime.getTime()) {
        lastTime = new Date(lastTime.getTime() + 60_000);
      } else {
        lastTime = stageTime;
      }

      const location = existing?.location
        || (index === maxFlowIndex - 1
          ? (`${params.shipment?.receiverAddress?.city || params.fallbackLocation || 'SmartShip Hub'}, ${params.shipment?.receiverAddress?.state || ''}`.replace(/,\s*$/, ''))
          : (params.fallbackLocation || 'SmartShip Hub'));

      result.push({
        eventId: existing?.eventId,
        trackingNumber: params.trackingNumber,
        status: stage,
        location,
        description: existing?.description || this.defaultStatusDescription(stage),
        timestamp: lastTime.toISOString()
      });
    }

    return result;
  }

  private pickSeedTime(values: Array<string | undefined | null>): Date {
    for (const value of values) {
      if (!value) continue;
      const dt = new Date(value);
      if (!Number.isNaN(dt.getTime())) {
        return dt;
      }
    }
    return new Date();
  }
}
