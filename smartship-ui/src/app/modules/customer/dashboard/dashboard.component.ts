import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ShipmentService } from '../../../core/services/shipment.service';
import { RateRequest, ShipmentResponse } from '../../../shared/models/shipment.model';
import { StatusBadgePipe } from '../../../shared/pipes/status-badge.pipe';
import { catchError, finalize, timeout } from 'rxjs/operators';
import { forkJoin, of } from 'rxjs';

@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, StatusBadgePipe],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
/**
 * Customer dashboard.
 * Loads the user's shipments, derives basic stats, and optionally calls the rate endpoint to show estimated costs.
 */
export class DashboardComponent implements OnInit {
  private shipmentService = inject(ShipmentService);
  private cdr = inject(ChangeDetectorRef);

  stats = {
    totalShipments: 0,
    inTransit: 0,
    delivered: 0,
    pending: 0
  };
  recentShipments: ShipmentResponse[] = [];
  estimatedCosts: Record<number, number | null> = {};
  isCostLoading: Record<number, boolean> = {};
  isLoading = true;
  errorMessage = '';
  private loadingGuardTimer: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    this.loadDashboard();
  }

  retryLoad(): void {
    this.loadDashboard();
  }

  private loadDashboard(): void {
    this.errorMessage = '';
    this.isLoading = true;
    if (this.loadingGuardTimer) {
      clearTimeout(this.loadingGuardTimer);
      this.loadingGuardTimer = null;
    }

    this.loadingGuardTimer = setTimeout(() => {
      if (this.isLoading) {
        this.errorMessage = 'Dashboard request timed out. Please try again.';
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    }, 12000);

    this.shipmentService.getMyShipmentsAll().pipe(
      timeout(10000),
      catchError((err) => {
        this.errorMessage = err.name === 'TimeoutError'
          ? 'Dashboard request timed out. Please try again.'
          : 'Failed to load dashboard shipments.';
        return of([] as ShipmentResponse[]);
      }),
      finalize(() => {
        this.isLoading = false;
        if (this.loadingGuardTimer) {
          clearTimeout(this.loadingGuardTimer);
          this.loadingGuardTimer = null;
        }
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (shipments: ShipmentResponse[]) => {
        const sortedShipments = [...shipments].sort((a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );

        this.stats.totalShipments = sortedShipments.length;
        this.stats.inTransit = sortedShipments.filter(s => s.status === 'InTransit' || s.status === 'OutForDelivery' || s.status === 'PickedUp').length;
        this.stats.delivered = sortedShipments.filter(s => s.status === 'Delivered').length;
        this.stats.pending = sortedShipments.filter(s => s.status === 'Draft' || s.status === 'Booked').length;
        this.recentShipments = sortedShipments.slice(0, 5);
        this.loadEstimatedCosts(this.recentShipments);
        this.cdr.detectChanges();
      }
    });
  }

  private loadEstimatedCosts(shipments: ShipmentResponse[]): void {
    if (shipments.length === 0) {
      return;
    }

    for (const shipment of shipments) {
      this.isCostLoading[shipment.shipmentId] = true;
      this.estimatedCosts[shipment.shipmentId] = null;
    }

    const requests = shipments.map((shipment) => {
      const totalWeight = shipment.packages.reduce((sum, pkg) => sum + Number(pkg.weight || 0), 0);
      const originCity = shipment.senderAddress?.city?.trim() || '';
      const destinationCity = shipment.receiverAddress?.city?.trim() || '';

      if (!originCity || !destinationCity || totalWeight <= 0) {
        this.isCostLoading[shipment.shipmentId] = false;
        return of({ shipmentId: shipment.shipmentId, price: null as number | null });
      }

      const payload: RateRequest = {
        originCity,
        destinationCity,
        weight: totalWeight,
        serviceType: 'Standard'
      };

      return this.shipmentService.calculateRate(payload).pipe(
        timeout(10000),
        catchError(() => of({ price: null })),
        finalize(() => {
          this.isCostLoading[shipment.shipmentId] = false;
        }),
      );
    });

    forkJoin(requests).subscribe({
      next: (results: Array<{ price: number | null } | { shipmentId: number; price: number | null }>) => {
        for (let index = 0; index < shipments.length; index++) {
          const shipmentId = shipments[index].shipmentId;
          const rawPrice = results[index]?.price;
          this.estimatedCosts[shipmentId] = typeof rawPrice === 'number' && Number.isFinite(rawPrice)
            ? rawPrice
            : null;
        }
        this.cdr.detectChanges();
      }
    });
  }

  getEstimatedDeliveryDate(shipment: ShipmentResponse): Date | null {
    const pickupDate = shipment.pickupSchedule?.pickupDate ? new Date(shipment.pickupSchedule.pickupDate) : null;
    if (pickupDate && !Number.isNaN(pickupDate.getTime())) {
      return pickupDate;
    }

    const createdDate = shipment.createdAt ? new Date(shipment.createdAt) : null;
    if (!createdDate || Number.isNaN(createdDate.getTime())) {
      return null;
    }

    const estimatedDate = new Date(createdDate);
    estimatedDate.setDate(estimatedDate.getDate() + this.getEstimatedDeliveryDays(shipment));
    return estimatedDate;
  }

  private getEstimatedDeliveryDays(shipment: ShipmentResponse): number {
    return shipment.packages.length > 1 ? 5 : 3;
  }
}
