import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ShipmentService } from '../../../core/services/shipment.service';
import { ShipmentResponse } from '../../../shared/models/shipment.model';
import { StatusBadgePipe } from '../../../shared/pipes/status-badge.pipe';
import { IstDatePipe } from '../../../shared/pipes/ist-date.pipe';

@Component({
  selector: 'app-shipments',
  standalone: true,
  imports: [CommonModule, RouterModule, StatusBadgePipe, IstDatePipe],
  templateUrl: './shipments.component.html',
  styleUrl: './shipments.component.css'
})
/**
 * Customer shipments list with server-side pagination.
 * Sorts results by creation time (newest first) for consistent display.
 */
export class ShipmentsComponent implements OnInit {
  private shipmentService = inject(ShipmentService);
  private cdr = inject(ChangeDetectorRef);
  private readonly defaultPageSize = 5;

  shipments: ShipmentResponse[] = [];
  isLoading = true;
  pageNumber = 1;
  pageSize = this.defaultPageSize;
  totalItems = 0;
  totalPages = 0;

  get hasPreviousPage(): boolean {
    return this.pageNumber > 1;
  }

  get hasNextPage(): boolean {
    return this.pageNumber < this.totalPages;
  }

  ngOnInit(): void {
    this.loadShipments(1);
  }

  loadShipments(pageNumber: number): void {
    this.isLoading = true;
    this.shipmentService.getMyShipmentsPage(pageNumber, this.pageSize).subscribe({
      next: (response) => {
        // Ensure shipments are sorted by CreatedAt descending (newest first)
        this.shipments = (response.data || []).sort((a, b) => {
          const dateA = new Date(a.createdAt || 0).getTime();
          const dateB = new Date(b.createdAt || 0).getTime();
          return dateB - dateA; // Descending order (newest first)
        });
        this.pageNumber = response.pageNumber;
        this.totalItems = response.totalItems;
        this.totalPages = response.totalPages;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.shipments = [];
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  goToPage(pageNumber: number): void {
    if (pageNumber < 1 || pageNumber > this.totalPages || pageNumber === this.pageNumber) {
      return;
    }

    this.loadShipments(pageNumber);
  }
}
