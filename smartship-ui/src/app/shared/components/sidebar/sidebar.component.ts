import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

/**
 * Role-aware sidebar navigation.
 * Exposes admin vs customer navigation items and supports collapsed + mobile drawer states.
 */
interface NavItem {
  icon: string;
  label: string;
  route: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  collapsed = signal(false);
  mobileOpen = signal(false);

  isAdmin(): boolean {
    return this.authService.getUserRole() === 'ADMIN';
  }

  get navItems(): NavItem[] {
    if (this.isAdmin()) {
      return [
        { icon: 'dashboard', label: 'Dashboard', route: '/admin/dashboard' },
        { icon: 'local_shipping', label: 'Shipments', route: '/admin/shipments' },
        { icon: 'warehouse', label: 'Hubs', route: '/admin/hubs' },
        { icon: 'group', label: 'Users', route: '/admin/users' },
        { icon: 'warning', label: 'Issues', route: '/admin/issues' },
        { icon: 'folder', label: 'Documents', route: '/admin/documents' },
        { icon: 'analytics', label: 'Reports', route: '/admin/reports' },
      ];
    }
    return [
      { icon: 'dashboard', label: 'Dashboard', route: '/customer/dashboard' },
      { icon: 'add_box', label: 'Create Shipment', route: '/shipment/create' },
      { icon: 'local_shipping', label: 'Shipments', route: '/customer/shipments' },
      { icon: 'location_on', label: 'Tracking', route: '/customer/tracking' },
    ];
  }

  get userName(): string {
    return this.authService.user()?.name || 'User';
  }

  get userRole(): string {
    return this.authService.getUserRole()?.toLowerCase() || 'user';
  }

  get userInitial(): string {
    const name = this.authService.user()?.name;
    return name ? name.charAt(0).toUpperCase() : 'U';
  }

  isActive(route: string): boolean {
    return this.router.url.startsWith(route);
  }

  toggleCollapse(): void {
    this.collapsed.update(v => !v);
  }

  toggleMobile(): void {
    this.mobileOpen.update(v => !v);
  }

  closeMobile(): void {
    this.mobileOpen.set(false);
  }

  onLogout(): void {
    this.closeMobile();
    this.authService.logout();
  }
}
