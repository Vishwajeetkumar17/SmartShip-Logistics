import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

/**
 * Root route table for the SmartShip UI.
 * Uses lazy loading for feature areas and enforces auth/role access at route boundaries.
 */
export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./modules/landing/landing.component').then(m => m.LandingComponent),
    pathMatch: 'full'
  },
  {
    path: 'auth',
    loadChildren: () => import('./modules/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: 'customer',
    loadChildren: () => import('./modules/customer/customer.routes').then(m => m.CUSTOMER_ROUTES),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['CUSTOMER'] }
  },
  {
    path: 'admin',
    loadChildren: () => import('./modules/admin/admin.routes').then(m => m.ADMIN_ROUTES),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN'] }
  },
  {
    path: 'shipment',
    loadChildren: () => import('./modules/shipment/shipment.routes').then(m => m.SHIPMENT_ROUTES),
    canActivate: [authGuard]
  },
  {
    path: 'profile',
    loadComponent: () => import('./modules/profile/profile.component').then(m => m.ProfileComponent),
    canActivate: [authGuard]
  },
  {
    path: 'change-password',
    loadComponent: () => import('./modules/profile/change-password/change-password.component').then(m => m.ChangePasswordComponent),
    canActivate: [authGuard]
  },
  {
    path: '**',
    loadComponent: () => import('./core/components/not-found/not-found').then(m => m.NotFound),
    data: { isNotFound: true }
  }
];
