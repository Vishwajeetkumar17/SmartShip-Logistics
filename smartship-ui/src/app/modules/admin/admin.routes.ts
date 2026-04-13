import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ShipmentsComponent } from './shipments/shipments.component';
import { ShipmentDetailsComponent } from './shipment-details/shipment-details.component';
import { HubsComponent } from './hubs/hubs.component';
import { ReportsComponent } from './reports/reports.component';
import { UsersComponent } from './users/users.component';
import { IssuesComponent } from './issues/issues.component';
import { DocumentsComponent } from './documents/documents.component';

/**
 * Admin area routes (dashboard, operations, monitoring, reports).
 */
export const ADMIN_ROUTES: Routes = [
  { path: 'dashboard', component: DashboardComponent },
  { path: 'shipments', component: ShipmentsComponent },
  { path: 'shipments/:id', component: ShipmentDetailsComponent },
  { path: 'hubs', component: HubsComponent },
  { path: 'reports', component: ReportsComponent },
  { path: 'users', component: UsersComponent },
  { path: 'issues', component: IssuesComponent },
  { path: 'documents', component: DocumentsComponent },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];
