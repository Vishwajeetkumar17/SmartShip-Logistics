import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ShipmentsComponent } from './shipments/shipments.component';
import { TrackingComponent } from './tracking/tracking.component';

/**
 * Customer area routes (dashboard, shipments, tracking).
 */
export const CUSTOMER_ROUTES: Routes = [
  { path: 'dashboard', component: DashboardComponent },
  { path: 'shipments', component: ShipmentsComponent },
  { path: 'tracking', component: TrackingComponent },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];
