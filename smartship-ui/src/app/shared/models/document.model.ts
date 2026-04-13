/**
 * Document models for the Angular client.
 * Mirrors DocumentService DTOs for uploads, metadata display, and delivery proof handling.
 */

// Matches backend: SmartShip.DocumentService DTOs

export interface DocumentResponse {
  documentId: number;
  shipmentId: number;
  customerId: number;
  fileName: string;
  fileUrl: string;
  documentType: string;
  contentType: string;
  uploadedAt: string;
}

export interface DeliveryProofResponse {
  proofId: number;
  shipmentId: number;
  fileUrl: string;
  signerName: string;
  notes: string;
  timestamp: string;
}
