import { Component, inject, OnInit, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule, Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SidebarComponent } from './shared/components/sidebar/sidebar.component';
import { AuthService } from './core/services/auth.service';
import { AdminService, ExceptionRecord } from './core/services/admin.service';
import { ShipmentService } from './core/services/shipment.service';
import { filter } from 'rxjs/operators';

export interface AppNotification {
  id: string;
  icon: string;
  iconColor: string;
  title: string;
  message: string;
  route: string[];
  queryParams?: any;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterModule, SidebarComponent, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private router = inject(Router);
  private activatedRoute = inject(ActivatedRoute);
  private authService = inject(AuthService);
  private adminService = inject(AdminService);
  private shipmentService = inject(ShipmentService);
  private elementRef = inject(ElementRef);

  searchQuery: string = '';
  showNotifications: boolean = false;
  isNotFound: boolean = false;
  openIssues: ExceptionRecord[] = [];
  appNotifications: AppNotification[] = [];

  ngOnInit() {
    this.fetchNotifications();
    
    // Close notifications on navigation
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.showNotifications = false;
      this.checkIfNotFound();
    });

    this.checkIfNotFound();
  }

  checkIfNotFound() {
    let route = this.activatedRoute.snapshot.root;
    while (route.firstChild) {
      route = route.firstChild;
    }
    this.isNotFound = !!route.data['isNotFound'];
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    const clickedInside = this.elementRef.nativeElement.querySelector('.notifications-wrapper')?.contains(event.target);
    if (!clickedInside && this.showNotifications) {
      this.showNotifications = false;
    }
  }

  get isAdmin(): boolean {
    return this.authService.getUserRole() === 'ADMIN';
  }

  formatStatus(status: string): string {
    if (!status) return 'Unknown';
    
    // Check known mappings case-insensitively
    const normalized = status.toLowerCase().replace(/[\s_-]/g, '');
    const statusMap: Record<string, string> = {
      'pending': 'Pending',
      'draft': 'Draft',
      'booked': 'Booked',
      'pickedup': 'Picked Up',
      'intransit': 'In Transit',
      'outfordelivery': 'Out for Delivery',
      'delivered': 'Delivered',
      'cancelled': 'Cancelled',
      'delayed': 'Delayed'
    };

    if (statusMap[normalized]) {
      return statusMap[normalized];
    }

    // Generic fallback for any CamelCase
    return status.replace(/([A-Z])/g, ' $1').trim();
  }

  fetchNotifications() {
    if (!this.authService.isAuthenticated()) {
      return;
    }

    if (this.isAdmin) {
      this.adminService.getIssues().subscribe({
        next: (issues: ExceptionRecord[]) => {
          this.openIssues = issues.filter((issue: ExceptionRecord) => issue.status === 'Open');
          this.appNotifications = this.openIssues.map((issue: ExceptionRecord) => ({
            id: `issue-${issue.exceptionId}`,
            icon: 'warning',
            iconColor: '#f59e0b',
            title: `Issue with Shipment SH${issue.shipmentId.toString().padStart(3, '0')}`,
            message: this.formatIssueType(issue.exceptionType),
            route: ['/admin/issues']
          }));
        },
        error: (err: any) => console.error('Failed to fetch issues for notifications', err)
      });
    } else {
      this.shipmentService.getMyShipmentsAll().subscribe({
        next: (shipments) => {
          const updatedShipments = shipments.filter(s => 
            ['PickedUp', 'InTransit', 'OutForDelivery', 'Delivered'].includes(String(s.status))
          )
          // Sort by creation desc (assuming higher ID = newer, or just take first 5)
          .sort((a,b) => b.shipmentId - a.shipmentId)
          .slice(0, 5);

          this.appNotifications = updatedShipments.map(s => {
            const isDelivered = String(s.status) === 'Delivered';
            return {
              id: `shipment-${s.shipmentId}`,
              icon: isDelivered ? 'check_circle' : 'local_shipping',
              iconColor: isDelivered ? '#10B981' : '#3B82F6',
              title: `Shipment SH${s.shipmentId.toString().padStart(3, '0')} Update`,
              message: `Status: ${this.formatStatus(String(s.status))}`,
              route: ['/customer/tracking'],
              queryParams: { track: s.trackingNumber }
            };
          });
        },
        error: (err) => console.error('Failed to fetch shipments for notifications', err)
      });
    }
  }

  onSearch(event: Event) {
    event.preventDefault();
    if (!this.searchQuery.trim()) return;

    if (this.isAdmin) {
      if (this.searchQuery.trim().toUpperCase().startsWith('SH')) {
         const id = Number(this.searchQuery.trim().substring(2));
         this.router.navigate(['/admin/shipments', isNaN(id) ? this.searchQuery : id]);
      } else {
        // Assume ID and route to details page or shipment search
        this.router.navigate(['/admin/shipments', this.searchQuery.trim()]);
      }
      this.searchQuery = '';
    }
  }

  toggleNotifications() {
    this.showNotifications = !this.showNotifications;
    if (this.showNotifications) {
      this.fetchNotifications();
    }
  }

  clearNotifications() {
    this.openIssues = [];
    this.appNotifications = [];
    this.showNotifications = false;
  }

  formatIssueType(type: string): string {
    if (!type) return 'Issue';
    
    // Add spaces between camelCase and replace -,_ with space
    const withSpaces = type
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .replace(/[_-]+/g, ' ')
      .replace(/\s+/g, ' ')
      .trim();

    // Capitalize first letter of each word
    return withSpaces.split(' ')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
      .join(' ')
      .replace(/Exception/i, 'Issue');
  }

  get isAuthenticatedPage(): boolean {
    if (this.isNotFound) return false;
    const url = this.router.url.split('?')[0];
    const publicRoutes = ['/', '/auth/login', '/auth/signup', '/auth/forgot-password'];
    return this.authService.isAuthenticated() && !publicRoutes.includes(url);
  }

  get pageTitle(): string {
    const path = this.router.url.toLowerCase();
    if (path.includes('/dashboard')) return 'Dashboard';
    if (path.includes('/shipments') || path.includes('/shipment-details')) return 'Shipments';
    if (path.includes('/shipment/create')) return 'Create Shipment';
    if (path.includes('/tracking')) return 'Tracking';
    if (path.includes('/hubs')) return 'Hubs';
    if (path.includes('/users')) return 'Users';
    if (path.includes('/issues')) return 'Issues';
    if (path.includes('/documents')) return 'Documents';
    if (path.includes('/reports')) return 'Reports';
    if (path.includes('/profile')) return 'Profile';
    if (path.includes('/change-password')) return 'Change Password';
    return 'SmartShip';
  }

  get userInitial(): string {
    const name = this.authService.user()?.name;
    return name ? name.charAt(0).toUpperCase() : 'U';
  }
}
