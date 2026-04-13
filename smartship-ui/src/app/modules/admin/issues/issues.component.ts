import { Component, ElementRef, HostListener, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, ExceptionRecord } from '../../../core/services/admin.service';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-admin-issues',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './issues.component.html',
  styleUrl: './issues.component.css'
})
/**
 * Admin exception/issue management screen.
 * Lists open exceptions, tracks a "recently resolved" snapshot list, and provides actions (resolve/delay/return).
 * Includes a lightweight shipment-id autocomplete for creating new exceptions.
 */
export class IssuesComponent implements OnInit {
  private adminService = inject(AdminService);
  private elementRef = inject(ElementRef<HTMLElement>);
  private readonly _issues = signal<ExceptionRecord[]>([]);
  private readonly _recentResolved = signal<ExceptionRecord[]>([]);
  private readonly _isLoading = signal(true);
  private readonly _error = signal('');
  private readonly _success = signal('');
  private readonly _showCreateForm = signal(false);
  private readonly _isPerformingAction = signal(false);

  get issues(): ExceptionRecord[] {
    return this._issues();
  }

  get recentResolvedIssues(): ExceptionRecord[] {
    return this._recentResolved();
  }

  get isLoading(): boolean {
    return this._isLoading();
  }

  get error(): string {
    return this._error();
  }

  get success(): string {
    return this._success();
  }

  // Action modals
  actionType: 'resolve' | 'delay' | 'return' | null = null;
  actionShipmentId: number = 0;
  actionReason: string = '';
  get isPerformingAction(): boolean {
    return this._isPerformingAction();
  }

  // Create exception (delay/return on a shipment)
  get showCreateForm(): boolean {
    return this._showCreateForm();
  }

  newShipmentId: number = 0;
  newExceptionType: 'delay' | 'return' = 'delay';
  newReason: string = '';
  createFieldErrors: Record<'shipmentId' | 'reason', string> = {
    shipmentId: '',
    reason: ''
  };
  shipmentSearchQuery = '';
  allShipmentIds: number[] = [];
  filteredShipmentIds: number[] = [];
  showShipmentSuggestions = false;
  actionFieldError = '';

  ngOnInit(): void {
    this.loadIssues();
    this.loadShipmentOptions();
  }

  loadIssues(): void {
    this._isLoading.set(true);
    this._error.set('');
    this.adminService.getIssues().pipe(
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to load issues.');
        return of([] as ExceptionRecord[]);
      }),
      finalize(() => this._isLoading.set(false))
    ).subscribe(data => {
      const apiIssues = Array.isArray(data) ? data : [];
      const apiResolved = apiIssues.filter(ex => this.isResolvedStatus(ex.status));
      const openIssues = apiIssues.filter(ex => this.isOpenStatus(ex.status));

      const recentResolved = this._recentResolved();
      const mergedResolved = [...apiResolved, ...recentResolved]
        .filter((ex, index, all) => all.findIndex(other => this.getExceptionKey(other) === this.getExceptionKey(ex)) === index)
        .sort((a, b) => this.getExceptionDateValue(b.resolvedAt || b.createdAt) - this.getExceptionDateValue(a.resolvedAt || a.createdAt))
        .slice(0, 5)
        .map(ex => ({ ...ex, status: 'Closed' }));

      this._recentResolved.set(mergedResolved);
      this._issues.set(openIssues);
    });
  }

  // Resolve action
  openAction(type: 'resolve' | 'delay' | 'return', shipmentId: number): void {
    this.actionType = type;
    this.actionShipmentId = shipmentId;
    this.actionReason = '';
    this.actionFieldError = '';
  }

  cancelAction(): void {
    this.actionType = null;
    this.actionShipmentId = 0;
    this.actionReason = '';
    this.actionFieldError = '';
  }

  performAction(): void {
    if (!this.actionReason.trim()) {
      this.actionFieldError = 'Reason is required.';
      return;
    }

    this.actionFieldError = '';
    this._isPerformingAction.set(true);
    this._error.set('');

    let obs$;
    if (this.actionType === 'resolve') {
      obs$ = this.adminService.resolveException(this.actionShipmentId, this.actionReason);
    } else if (this.actionType === 'delay') {
      obs$ = this.adminService.delayShipment(this.actionShipmentId, this.actionReason);
    } else {
      obs$ = this.adminService.returnShipment(this.actionShipmentId, this.actionReason);
    }

    obs$.pipe(
      catchError(err => {
        this._error.set(err.error?.message || `Failed to ${this.actionType} exception.`);
        return of(null);
      }),
      finalize(() => this._isPerformingAction.set(false))
    ).subscribe(result => {
      if (result !== null) {
        if (this.actionType === 'resolve') {
          const resolvedRow = this.createResolvedSnapshot(this.actionShipmentId, this.actionReason);
          if (resolvedRow) {
            const merged = [resolvedRow, ...this._recentResolved()]
              .filter((ex, index, all) => all.findIndex(other => this.getExceptionKey(other) === this.getExceptionKey(ex)) === index)
              .slice(0, 5);
            this._recentResolved.set(merged);
          }
        }

        this._success.set(`Exception ${this.actionType}d successfully!`);
        this.cancelAction();
        this.loadIssues();
        setTimeout(() => this._success.set(''), 3000);
      }
    });
  }

  // Create new exception (for a shipment)
  toggleCreateForm(): void {
    this._showCreateForm.set(!this._showCreateForm());
    if (!this._showCreateForm()) {
      this.newShipmentId = 0;
      this.newReason = '';
      this.clearCreateFieldErrors();
      this.shipmentSearchQuery = '';
      this.showShipmentSuggestions = false;
    } else {
      this.filterShipmentOptions();
    }
  }

  submitNewException(): void {
    this._error.set('');
    this.clearCreateFieldErrors();

    if (!this.newShipmentId) {
      this.createFieldErrors.shipmentId = 'Shipment ID is required.';
    }

    if (!this.newReason.trim()) {
      this.createFieldErrors.reason = 'Reason is required.';
    }

    if (this.createFieldErrors.shipmentId || this.createFieldErrors.reason) {
      return;
    }
    this._isPerformingAction.set(true);
    this._error.set('');

    const obs$ = this.newExceptionType === 'delay'
      ? this.adminService.delayShipment(this.newShipmentId, this.newReason)
      : this.adminService.returnShipment(this.newShipmentId, this.newReason);

    obs$.pipe(
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to create exception.');
        return of(null);
      }),
      finalize(() => this._isPerformingAction.set(false))
    ).subscribe(result => {
      if (result !== null) {
        this._success.set('Exception created!');
        this._showCreateForm.set(false);
        this.newShipmentId = 0;
        this.newReason = '';
        this.clearCreateFieldErrors();
        this.shipmentSearchQuery = '';
        this.showShipmentSuggestions = false;
        this.loadIssues();
        setTimeout(() => this._success.set(''), 3000);
      }
    });
  }

  onShipmentQueryInput(value: string): void {
    this.createFieldErrors.shipmentId = '';
    this._error.set('');
    this.shipmentSearchQuery = value;
    this.showShipmentSuggestions = true;
    this.filterShipmentOptions();

    const numericValue = Number(value);
    this.newShipmentId = Number.isInteger(numericValue) && numericValue > 0 ? numericValue : 0;
  }

  toggleShipmentSuggestions(): void {
    this.showShipmentSuggestions = !this.showShipmentSuggestions;
    if (this.showShipmentSuggestions) {
      this.filterShipmentOptions();
    }
  }

  selectShipmentId(shipmentId: number): void {
    this.newShipmentId = shipmentId;
    this.shipmentSearchQuery = shipmentId.toString();
    this.createFieldErrors.shipmentId = '';
    this.showShipmentSuggestions = false;
  }

  onNewReasonInput(value: string): void {
    this.newReason = value;
    this.createFieldErrors.reason = '';
    this._error.set('');
  }

  onActionReasonInput(value: string): void {
    this.actionReason = value;
    this.actionFieldError = '';
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement | null;
    if (!target) {
      return;
    }

    const clickedInside = !!target.closest('.shipment-autocomplete');
    const belongsToComponent = this.elementRef.nativeElement.contains(target);
    if (belongsToComponent && !clickedInside) {
      this.showShipmentSuggestions = false;
    }
  }

  getTypeClass(type: string): string {
    const normalized = (type || '').toLowerCase();
    if (normalized.includes('delay')) {
      return 'type-delay';
    }
    if (normalized.includes('return')) {
      return 'type-return';
    }
    if (normalized.includes('cancel')) {
      return 'type-return';
    }
    if (normalized.includes('issue')) {
      return 'type-exception';
    }
    return normalized.includes('exception') ? 'type-exception' : 'type-default';
  }

  formatExceptionType(type: string): string {
    const raw = (type || '').trim();
    if (!raw) {
      return 'Exception';
    }

    const withSpaces = raw
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .replace(/[_-]+/g, ' ')
      .replace(/\s+/g, ' ')
      .trim();

    return withSpaces
      .split(' ')
      .filter(Boolean)
      .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
      .join(' ');
  }

  getStatusClass(status: string): string {
    return this.isOpenStatus(status) ? 'status-open' : 'status-resolved';
  }

  private isOpenStatus(status: string): boolean {
    return (status || '').toLowerCase() === 'open';
  }

  private isResolvedStatus(status: string): boolean {
    return !this.isOpenStatus(status);
  }

  private getExceptionDateValue(date?: string): number {
    if (!date) {
      return 0;
    }
    const timestamp = new Date(date).getTime();
    return Number.isNaN(timestamp) ? 0 : timestamp;
  }

  private getExceptionKey(exception: ExceptionRecord): string {
    if (exception.exceptionId) {
      return `id-${exception.exceptionId}`;
    }
    return `shipment-${exception.shipmentId}-${(exception.exceptionType || '').toLowerCase()}`;
  }

  private createResolvedSnapshot(shipmentId: number, resolutionNotes: string): ExceptionRecord | null {
    const source = this._issues().find(ex => ex.shipmentId === shipmentId && this.isOpenStatus(ex.status));
    if (!source) {
      return null;
    }

    return {
      ...source,
      description: source.description || resolutionNotes,
      status: 'Closed',
      resolvedAt: new Date().toISOString()
    };
  }

  private loadShipmentOptions(): void {
    this.adminService.getShipments().pipe(
      catchError(() => of([]))
    ).subscribe(shipments => {
      const ids = (Array.isArray(shipments) ? shipments : [])
        .map(shipment => shipment?.shipmentId)
        .filter((shipmentId): shipmentId is number => typeof shipmentId === 'number' && shipmentId > 0);

      this.allShipmentIds = Array.from(new Set(ids)).sort((a, b) => b - a);
      this.filterShipmentOptions();
    });
  }

  filterShipmentOptions(): void {
    const query = (this.shipmentSearchQuery || '').trim().toLowerCase();
    this.filteredShipmentIds = query
      ? this.allShipmentIds.filter(shipmentId => shipmentId.toString().toLowerCase().includes(query))
      : [...this.allShipmentIds];
  }

  private clearCreateFieldErrors(): void {
    this.createFieldErrors = {
      shipmentId: '',
      reason: ''
    };
  }
}
