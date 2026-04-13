import { ChangeDetectorRef, Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ShipmentService } from '../../../core/services/shipment.service';
import { ShipmentResponse } from '../../../shared/models/shipment.model';
import { StatusBadgePipe } from '../../../shared/pipes/status-badge.pipe';
import { catchError, finalize, timeout } from 'rxjs/operators';
import { of, Subscription } from 'rxjs';

@Component({
  selector: 'app-shipment-details',
  standalone: true,
  imports: [CommonModule, RouterModule, StatusBadgePipe],
  templateUrl: './details.component.html',
  styleUrl: './details.component.css'
})
/**
 * Shipment details screen.
 * Loads a shipment by route id and includes explicit timeout/guard logic to avoid hanging spinners.
 */
export class DetailsComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private shipmentService = inject(ShipmentService);
  private cdr = inject(ChangeDetectorRef);

  shipment: ShipmentResponse | null = null;
  isLoading = true;
  errorMessage = '';
  private loadSubscription: Subscription | null = null;
  private loadingGuardTimer: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    this.loadShipment();
  }

  ngOnDestroy(): void {
    this.clearLoadGuards();
  }

  retryLoad(): void {
    this.loadShipment();
  }

  private loadShipment(): void {
    this.clearLoadGuards();
    this.shipment = null;
    this.errorMessage = '';
    this.isLoading = true;

    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.isLoading = false;
      this.errorMessage = 'Invalid shipment id';
      return;
    }

    this.loadingGuardTimer = setTimeout(() => {
      if (this.isLoading) {
        this.errorMessage = 'Shipment details request timed out.';
        this.isLoading = false;
        this.loadSubscription?.unsubscribe();
      }
    }, 12000);

    this.loadSubscription = this.shipmentService.getById(id).pipe(
      timeout(10000),
      catchError((err) => {
        this.errorMessage = err.name === 'TimeoutError'
          ? 'Shipment details request timed out.'
          : err.status === 404
            ? 'Shipment not found'
            : 'Failed to load shipment';
        return of(null);
      }),
      finalize(() => {
        this.isLoading = false;
        this.clearGuardTimer();
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (data) => {
        if (!data) {
          return;
        }
        this.shipment = data;
      }
    });
  }

  private clearLoadGuards(): void {
    this.loadSubscription?.unsubscribe();
    this.loadSubscription = null;
    this.clearGuardTimer();
  }

  private clearGuardTimer(): void {
    if (this.loadingGuardTimer) {
      clearTimeout(this.loadingGuardTimer);
      this.loadingGuardTimer = null;
    }
  }
}
