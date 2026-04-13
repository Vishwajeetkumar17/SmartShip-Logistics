import { Component, ElementRef, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
/**
 * Top navigation bar with responsive (mobile) menu behavior.
 * Detects admin role for navigation and closes the mobile menu on outside clicks/resizes.
 */
export class NavbarComponent {
  authService = inject(AuthService);
  private router = inject(Router);
  private elementRef = inject(ElementRef<HTMLElement>);
  mobileMenuOpen = false;

  isAdmin(): boolean {
    return this.authService.getUserRole() === 'ADMIN';
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.mobileMenuOpen) {
      return;
    }

    const target = event.target as Node | null;
    if (target && !this.elementRef.nativeElement.contains(target)) {
      this.closeMobileMenu();
    }
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    if (window.innerWidth > 1024 && this.mobileMenuOpen) {
      this.closeMobileMenu();
    }
  }

  getCurrentNavLabel(): string {
    const path = this.router.url.toLowerCase();

    if (path.includes('/dashboard')) return 'Dashboard';
    if (path.includes('/shipments')) return 'Shipments';
    if (path.includes('/hubs')) return 'Hubs';
    if (path.includes('/users')) return 'Users';
    if (path.includes('/issues')) return 'Issues';
    if (path.includes('/documents')) return 'Documents';
    if (path.includes('/reports')) return 'Reports';
    if (path.includes('/shipment/create')) return 'Create Shipment';
    if (path.includes('/profile')) return 'Profile';

    return 'Menu';
  }

  onLogout(): void {
    this.closeMobileMenu();
    this.authService.logout();
  }
}
