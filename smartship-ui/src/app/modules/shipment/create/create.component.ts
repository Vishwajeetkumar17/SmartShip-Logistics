import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ShipmentService } from '../../../core/services/shipment.service';
import { CreateShipmentRequest, Address } from '../../../shared/models/shipment.model';
import { timeout, catchError, finalize } from 'rxjs/operators';
import { throwError } from 'rxjs';

type PackageForm = {
  weight: number | null;
  length: number | null;
  width: number | null;
  height: number | null;
  description: string;
};

@Component({
  selector: 'app-create-shipment',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './create.component.html',
  styleUrl: './create.component.css'
})
/**
 * Multi-step shipment booking wizard for customers.
 * Validates each step, computes a local estimated rate (and can be extended to call backend rate endpoints),
 * then submits a CreateShipment request via the gateway.
 */
export class CreateComponent {
  private shipmentService = inject(ShipmentService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  currentStep = 1;
  totalSteps = 4;
  isLoading = false;
  isRateLoading = false;
  errorMessage = '';
  rateErrorMessage = '';
  estimatedCost: number | null = null;

  senderName = '';
  senderEmail = '';
  senderPhone = '';
  senderAddress: Address = { street: '', city: '', state: '', country: 'India', postalCode: '' };

  receiverName = '';
  receiverEmail = '';
  receiverPhone = '';
  receiverAddress: Address = { street: '', city: '', state: '', country: 'India', postalCode: '' };

  serviceType = 'Standard';
  packageType = 'Box';
  pkg: PackageForm = { weight: null, length: null, width: null, height: null, description: '' };
  validationErrors: Record<string, string> = {};

  getFieldError(field: string): string {
    return this.validationErrors[field] ?? '';
  }

  onFieldInput(field: string): void {
    if (this.validationErrors[field]) {
      delete this.validationErrors[field];
      this.validationErrors = { ...this.validationErrors };
    }
    this.errorMessage = '';
  }

  nextStep(): void {
    const validationError = this.validateStep(this.currentStep);
    if (validationError) {
      return;
    }

    this.errorMessage = '';

    if (this.currentStep < this.totalSteps) {
      this.currentStep++;
      if (this.currentStep === 4) {
        this.loadEstimatedRate();
      }
    }
  }

  prevStep(): void {
    this.errorMessage = '';
    this.validationErrors = {};
    if (this.currentStep > 1) this.currentStep--;
  }

  onSubmit(): void {
    const validationError = this.validateForm();
    if (validationError) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const request: CreateShipmentRequest = {
      senderName: this.senderName.trim(),
      senderPhone: this.senderPhone.trim(),
      receiverName: this.receiverName.trim(),
      receiverPhone: this.receiverPhone.trim(),
      serviceType: this.normalizeServiceType(this.serviceType),
      estimatedCost: this.estimatedCost ?? this.calculateEstimatedCost(),
      senderAddress: {
        ...this.senderAddress,
        street: this.senderAddress.street.trim(),
        city: this.senderAddress.city.trim(),
        state: this.senderAddress.state.trim(),
        country: this.senderAddress.country.trim(),
        postalCode: this.senderAddress.postalCode.trim()
      },
      receiverAddress: {
        ...this.receiverAddress,
        street: this.receiverAddress.street.trim(),
        city: this.receiverAddress.city.trim(),
        state: this.receiverAddress.state.trim(),
        country: this.receiverAddress.country.trim(),
        postalCode: this.receiverAddress.postalCode.trim()
      },
      packages: [{
        weight: Number(this.pkg.weight ?? 0),
        length: Number(this.pkg.length ?? 0),
        width: Number(this.pkg.width ?? 0),
        height: Number(this.pkg.height ?? 0),
        description: this.pkg.description.trim()
      }]
    };

    this.shipmentService.create(request).pipe(
      timeout(10000),
      catchError(err => {
        if (err.name === 'TimeoutError') {
          return throwError(() => ({ error: { message: 'Server is not responding.' } }));
        }
        if (err.status === 0) {
          return throwError(() => ({ error: { message: 'Cannot connect to server.' } }));
        }
        return throwError(() => err);
      }),
      finalize(() => { this.isLoading = false; this.cdr.detectChanges(); })
    ).subscribe({
      next: (response) => {
        this.router.navigate(['/shipment', response.shipmentId]);
      },
      error: (err) => {
        this.errorMessage = err.error?.detail || err.error?.message || err.error?.title || 'Failed to create shipment';
      }
    });
  }

  get estimatedDeliveryText(): string {
    switch (this.normalizeServiceType(this.serviceType)) {
      case 'Express':
        return 'Estimated delivery: 1-2 business days';
      case 'Economy':
        return 'Estimated delivery: 5-7 business days';
      default:
        return 'Estimated delivery: 3-5 business days';
    }
  }

  private loadEstimatedRate(): void {
    this.rateErrorMessage = '';
    this.estimatedCost = null;

    const senderCity = this.senderAddress.city.trim();
    const receiverCity = this.receiverAddress.city.trim();

    if (!senderCity || !receiverCity || !this.pkg.weight || this.pkg.weight <= 0) {
      this.rateErrorMessage = 'Enter sender city, receiver city, and package weight to calculate estimated cost.';
      return;
    }

    this.estimatedCost = this.calculateEstimatedCost();
  }

  private calculateEstimatedCost(): number {
    const weight = Number(this.pkg.weight ?? 0);
    const safeWeight = Number.isFinite(weight) && weight > 0 ? weight : 0;
    const baseRate = 100;
    const perKgRate = 50;
    const serviceMultiplier = this.getServiceMultiplier(this.normalizeServiceType(this.serviceType));
    const calculatedCost = (baseRate + (safeWeight * perKgRate)) * serviceMultiplier;
    return Math.round(calculatedCost * 100) / 100;
  }

  private getServiceMultiplier(serviceType: 'Standard' | 'Express' | 'Economy'): number {
    switch (serviceType) {
      case 'Express':
        return 1.5; // Express is 50% more expensive
      case 'Economy':
        return 0.8; // Economy is 20% cheaper
      default:
        return 1.0; // Standard is base rate
    }
  }
  private normalizeServiceType(value: string): 'Standard' | 'Express' | 'Economy' {
    if (value === 'Express' || value === 'Economy') {
      return value;
    }
    return 'Standard';
  }

  private validateForm(): string {
    const invalidStep = this.getFirstInvalidStep();
    if (!invalidStep) {
      this.validationErrors = {};
      return '';
    }

    this.currentStep = invalidStep;
    return this.validateStep(invalidStep);
  }

  private getFirstInvalidStep(): number | null {
    for (const step of [1, 2, 3]) {
      if (Object.keys(this.getValidationErrorsForStep(step)).length > 0) {
        return step;
      }
    }

    return null;
  }

  private validateStep(step: number): string {
    this.validationErrors = this.getValidationErrorsForStep(step);
    const firstError = Object.values(this.validationErrors)[0];
    return firstError ?? '';
  }

  private getValidationErrorsForStep(step: number): Record<string, string> {
    if (step === 1) {
      return this.validateSenderStep();
    }

    if (step === 2) {
      return this.validateReceiverStep();
    }

    if (step === 3) {
      return this.validatePackageStep();
    }

    return {};
  }

  private validateSenderStep(): Record<string, string> {
    const errors: Record<string, string> = {};
    const senderName = this.senderName.trim();

    if (!senderName) {
      errors['senderName'] = 'Sender full name is required.';
    } else if (senderName.length < 2) {
      errors['senderName'] = 'Sender full name is required (minimum 2 characters).';
    } else if (!this.isValidPersonName(senderName)) {
      errors['senderName'] = 'Sender name should contain letters only (no numbers).';
    }

    if (!this.senderEmail.trim()) {
      errors['senderEmail'] = 'Sender email is required.';
    } else if (!this.isValidEmail(this.senderEmail)) {
      errors['senderEmail'] = 'Enter a valid sender email address.';
    }

    if (!this.senderPhone.trim()) {
      errors['senderPhone'] = 'Sender phone number is required.';
    } else if (!this.isValidPhone(this.senderPhone)) {
      errors['senderPhone'] = 'Sender phone number must be exactly 10 digits.';
    }

    if (!this.senderAddress.street.trim()) {
      errors['senderStreet'] = 'Sender street address is required.';
    }

    if (!this.senderAddress.city.trim()) {
      errors['senderCity'] = 'Sender city is required.';
    }

    if (!this.senderAddress.state.trim()) {
      errors['senderState'] = 'Sender state is required.';
    }

    if (!this.senderAddress.postalCode.trim()) {
      errors['senderPostalCode'] = 'Sender postal code is required.';
    } else if (!this.isValidPostalCode(this.senderAddress.postalCode)) {
      errors['senderPostalCode'] = 'Sender ZIP code must be exactly 6 digits (numbers only).';
    }

    return errors;
  }

  private validateReceiverStep(): Record<string, string> {
    const errors: Record<string, string> = {};
    const receiverName = this.receiverName.trim();

    if (!receiverName) {
      errors['receiverName'] = 'Receiver full name is required.';
    } else if (receiverName.length < 2) {
      errors['receiverName'] = 'Receiver full name is required (minimum 2 characters).';
    } else if (!this.isValidPersonName(receiverName)) {
      errors['receiverName'] = 'Receiver name should contain letters only (no numbers).';
    }

    if (!this.receiverEmail.trim()) {
      errors['receiverEmail'] = 'Receiver email is required.';
    } else if (!this.isValidEmail(this.receiverEmail)) {
      errors['receiverEmail'] = 'Enter a valid receiver email address.';
    }

    if (!this.receiverPhone.trim()) {
      errors['receiverPhone'] = 'Receiver phone number is required.';
    } else if (!this.isValidPhone(this.receiverPhone)) {
      errors['receiverPhone'] = 'Receiver phone number must be exactly 10 digits.';
    }

    if (!this.receiverAddress.street.trim()) {
      errors['receiverStreet'] = 'Receiver street address is required.';
    }

    if (!this.receiverAddress.city.trim()) {
      errors['receiverCity'] = 'Receiver city is required.';
    }

    if (!this.receiverAddress.state.trim()) {
      errors['receiverState'] = 'Receiver state is required.';
    }

    if (!this.receiverAddress.postalCode.trim()) {
      errors['receiverPostalCode'] = 'Receiver postal code is required.';
    } else if (!this.isValidPostalCode(this.receiverAddress.postalCode)) {
      errors['receiverPostalCode'] = 'Receiver ZIP code must be exactly 6 digits (numbers only).';
    }

    return errors;
  }

  private validatePackageStep(): Record<string, string> {
    const errors: Record<string, string> = {};
    const validServices = ['Standard', 'Express', 'Economy'];

    if (!validServices.includes(this.serviceType)) {
      errors['serviceType'] = 'Please select a valid service type.';
    }

    if (!this.packageType.trim()) {
      errors['packageType'] = 'Please select a package type.';
    }

    if (!this.isPositiveNumber(this.pkg.weight)) {
      errors['weight'] = 'Package weight must be greater than 0.';
    }

    if (!this.isPositiveNumber(this.pkg.length)) {
      errors['length'] = 'Package length must be greater than 0.';
    }

    if (!this.isPositiveNumber(this.pkg.width)) {
      errors['width'] = 'Package width must be greater than 0.';
    }

    if (!this.isPositiveNumber(this.pkg.height)) {
      errors['height'] = 'Package height must be greater than 0.';
    }

    if (this.pkg.description.trim().length < 3) {
      errors['description'] = 'Package description is required (minimum 3 characters).';
    }

    return errors;
  }

  private isPositiveNumber(value: number | null): boolean {
    return value !== null && Number.isFinite(value) && value > 0;
  }

  private isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim());
  }

  private isValidPhone(phone: string): boolean {
    return /^\d{10}$/.test(phone.trim());
  }

  private isValidPostalCode(postalCode: string): boolean {
    return /^\d{6}$/.test(postalCode.trim());
  }

  private isValidPersonName(name: string): boolean {
    return /^[A-Za-z][A-Za-z\s.'-]*$/.test(name.trim());
  }
}
