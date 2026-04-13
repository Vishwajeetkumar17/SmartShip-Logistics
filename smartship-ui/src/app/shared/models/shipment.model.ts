/**
 * Shipment domain models for the Angular client.
 * These interfaces mirror ShipmentService DTOs and are shared across customer/admin screens.
 */

// Matches backend: SmartShip.ShipmentService models and DTOs

// Matches ShipmentStatus enum from backend
export type ShipmentStatus =
  | 'Draft'
  | 'Booked'
  | 'PickedUp'
  | 'InTransit'
  | 'OutForDelivery'
  | 'Delivered';

export type ShipmentServiceType = 'Standard' | 'Express' | 'Economy';

// Matches Address model from ShipmentService
export interface Address {
  addressId?: number;
  street: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
}

// Matches PackageDTO from backend
export interface Package {
  id?: number;
  weight: number;
  length: number;
  width: number;
  height: number;
  description: string;
}

// Matches PickupScheduleDTO
export interface PickupSchedule {
  pickupDate: string;
  notes: string;
}

// Matches CreateShipmentDTO from backend
export interface CreateShipmentRequest {
  senderName?: string;
  senderPhone?: string;
  receiverName?: string;
  receiverPhone?: string;
  serviceType?: ShipmentServiceType;
  estimatedCost?: number;
  senderAddress: Address;
  receiverAddress: Address;
  packages: Package[];
  pickupSchedule?: PickupSchedule;
}

// Matches UpdateShipmentDTO
export interface UpdateShipmentRequest {
  senderAddress?: Address;
  receiverAddress?: Address;
  pickupSchedule?: PickupSchedule;
}

// Matches ShipmentResponseDTO from backend
export interface ShipmentResponse {
  shipmentId: number;
  trackingNumber: string;
  customerId: number;
  customerName?: string;
  senderName?: string;
  senderPhone?: string;
  receiverName?: string;
  receiverPhone?: string;
  serviceType?: ShipmentServiceType;
  estimatedCost?: number;
  status: ShipmentStatus;
  totalWeight: number;
  createdAt: string;
  senderAddress: Address;
  receiverAddress: Address;
  packages: Package[];
  pickupSchedule?: PickupSchedule;
}

// Matches RateRequestDTO from backend
export interface RateRequest {
  originCity: string;
  destinationCity: string;
  weight: number;
  serviceType: 'Standard' | 'Express' | 'Economy';
}

export interface RateResponse {
  price: number;
}

export interface ShippingService {
  name: string;
  delivery: string;
}
