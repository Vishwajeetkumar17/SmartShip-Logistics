import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, HubResponse, CreateHubRequest } from '../../../core/services/admin.service';
import { LoaderComponent } from '../../../shared/components/loader/loader.component';

@Component({
  selector: 'app-hubs',
  standalone: true,
  imports: [CommonModule, FormsModule, LoaderComponent],
  templateUrl: './hubs.component.html',
  styleUrl: './hubs.component.css'
})
/**
 * Admin hub management screen.
 * Provides CRUD for hubs with client-side validation and optimistic reload on save/delete.
 */
export class HubsComponent implements OnInit {
  private adminService = inject(AdminService);
  private readonly _hubs = signal<HubResponse[]>([]);
  private readonly _isLoading = signal(true);
  private readonly _showForm = signal(false);
  private readonly _editingHubId = signal<number | null>(null);
  private readonly _errorMessage = signal('');
  private readonly _submitAttempted = signal(false);

  get hubs(): HubResponse[] {
    return this._hubs();
  }

  get isLoading(): boolean {
    return this._isLoading();
  }

  get showForm(): boolean {
    return this._showForm();
  }

  get editingHubId(): number | null {
    return this._editingHubId();
  }

  get errorMessage(): string {
    return this._errorMessage();
  }

  get submitAttempted(): boolean {
    return this._submitAttempted();
  }

  fieldTouched: Record<'name' | 'managerName' | 'address' | 'contactNumber' | 'email', boolean> = {
    name: false,
    managerName: false,
    address: false,
    contactNumber: false,
    email: false
  };

  hubForm: CreateHubRequest = {
    name: '',
    address: '',
    contactNumber: '',
    managerName: '',
    email: '',
    isActive: true
  };

  ngOnInit(): void {
    this.loadHubs();
  }

  loadHubs(): void {
    this._isLoading.set(true);
    this.adminService.getHubs().subscribe({
      next: (data) => {
        this._hubs.set(data);
        this._isLoading.set(false);
      },
      error: () => {
        this._isLoading.set(false);
      }
    });
  }

  openCreateForm(): void {
    this._editingHubId.set(null);
    this.hubForm = { name: '', address: '', contactNumber: '', managerName: '', email: '', isActive: true };
    this.resetValidationState();
    this._showForm.set(true);
  }

  openEditForm(hub: HubResponse): void {
    this._editingHubId.set(hub.hubId);
    this.hubForm = {
      name: hub.name,
      address: hub.address,
      contactNumber: hub.contactNumber,
      managerName: hub.managerName,
      email: hub.email || '',
      isActive: hub.isActive
    };
    this.resetValidationState();
    this._showForm.set(true);
  }

  saveHub(): void {
    this._submitAttempted.set(true);
    this._errorMessage.set('');
    if (!this.validateHubForm()) {
      return;
    }

    const payload: CreateHubRequest = {
      name: this.hubForm.name.trim(),
      address: this.hubForm.address.trim(),
      managerName: (this.hubForm.managerName || '').trim(),
      contactNumber: (this.hubForm.contactNumber || '').trim(),
      email: (this.hubForm.email || '').trim(),
      isActive: !!this.hubForm.isActive
    };

    const editingHubId = this._editingHubId();
    if (editingHubId) {
      this.adminService.updateHub(editingHubId, payload).subscribe({
        next: () => {
          this._showForm.set(false);
          this.loadHubs();
        },
        error: (err) => {
          this._errorMessage.set(err.error?.message || 'Failed to update hub');
        }
      });
    } else {
      this.adminService.createHub(payload).subscribe({
        next: () => {
          this._showForm.set(false);
          this.loadHubs();
        },
        error: (err) => {
          this._errorMessage.set(err.error?.message || 'Failed to create hub');
        }
      });
    }
  }

  deleteHub(id: number): void {
    if (confirm('Are you sure you want to delete this hub?')) {
      this.adminService.deleteHub(id).subscribe({
        next: () => this.loadHubs(),
        error: (err) => {
          this._errorMessage.set(err.error?.message || 'Failed to delete hub');
        }
      });
    }
  }

  cancelForm(): void {
    this._showForm.set(false);
    this._errorMessage.set('');
    this.resetValidationState();
  }

  isHubFormValid(): boolean {
    return !this.getHubNameError()
      && !this.getManagerNameError()
      && !this.getAddressError()
      && !this.getContactNumberError()
      && !this.getEmailError();
  }

  onPhoneInput(value: string): void {
    this.hubForm.contactNumber = String(value ?? '').replace(/\D/g, '').slice(0, 10);
    this._errorMessage.set('');
  }

  markFieldTouched(field: 'name' | 'managerName' | 'address' | 'contactNumber' | 'email'): void {
    this.fieldTouched[field] = true;
  }

  showFieldError(field: 'name' | 'managerName' | 'address' | 'contactNumber' | 'email'): boolean {
    const hasError = this.getFieldError(field) !== '';
    return hasError && (this.fieldTouched[field] || this._submitAttempted());
  }

  getFieldError(field: 'name' | 'managerName' | 'address' | 'contactNumber' | 'email'): string {
    if (field === 'name') return this.getHubNameError();
    if (field === 'managerName') return this.getManagerNameError();
    if (field === 'address') return this.getAddressError();
    if (field === 'contactNumber') return this.getContactNumberError();
    return this.getEmailError();
  }

  private validateHubForm(): boolean {
    return this.getValidationError() === '';
  }

  private getValidationError(): string {
    const hubNameError = this.getHubNameError();
    if (hubNameError) return hubNameError;

    const managerNameError = this.getManagerNameError();
    if (managerNameError) return managerNameError;

    const addressError = this.getAddressError();
    if (addressError) return addressError;

    const phoneError = this.getContactNumberError();
    if (phoneError) return phoneError;

    const emailError = this.getEmailError();
    if (emailError) return emailError;

    return '';
  }

  private getHubNameError(): string {
    const hubName = String(this.hubForm.name ?? '').trim();
    const hubNamePattern = /^[A-Za-z0-9][A-Za-z0-9\s.'-]{1,99}$/;
    if (!hubName) return 'Hub name is required.';
    if (!hubNamePattern.test(hubName)) {
      return 'Hub name must be 2-100 characters and can contain letters, numbers, spaces, dot, apostrophe, and hyphen.';
    }
    return '';
  }

  private getManagerNameError(): string {
    const managerName = String(this.hubForm.managerName ?? '').trim();
    const managerNamePattern = /^[A-Za-z][A-Za-z\s.'-]{1,99}$/;
    if (!managerName) return 'Manager name is required.';
    if (!managerNamePattern.test(managerName)) {
      return 'Manager name must be 2-100 characters and contain only letters/spaces.';
    }
    return '';
  }

  private getAddressError(): string {
    const address = String(this.hubForm.address ?? '').trim();
    if (!address) return 'Address is required.';
    return '';
  }

  private getContactNumberError(): string {
    const contactNumber = String(this.hubForm.contactNumber ?? '').trim();
    const phonePattern = /^\d{10}$/;
    if (!phonePattern.test(contactNumber)) return 'Contact number must be exactly 10 digits.';
    return '';
  }

  private getEmailError(): string {
    const email = String(this.hubForm.email ?? '').trim();
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;
    if (!email) return 'Email address is required.';
    if (!emailPattern.test(email)) return 'Please enter a valid email address.';
    return '';
  }

  private resetValidationState(): void {
    this._submitAttempted.set(false);
    this.fieldTouched = {
      name: false,
      managerName: false,
      address: false,
      contactNumber: false,
      email: false
    };
  }
}
