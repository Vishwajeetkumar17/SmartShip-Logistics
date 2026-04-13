import { Routes } from '@angular/router';
import { CreateComponent } from './create/create.component';
import { DetailsComponent } from './details/details.component';

/**
 * Shipment feature routes (create flow + shipment details).
 */
export const SHIPMENT_ROUTES: Routes = [
  { path: 'create', component: CreateComponent },
  { path: ':id', component: DetailsComponent },
  { path: '', redirectTo: 'create', pathMatch: 'full' }
];
