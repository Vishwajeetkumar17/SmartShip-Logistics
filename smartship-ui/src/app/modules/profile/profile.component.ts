import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UpdateProfileRequest } from '../../shared/models/user.model';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
/**
 * User profile screen for viewing basic identity information and updating name/phone.
 * Uses the AuthService's current-user signal as the display source.
 */
export class ProfileComponent implements OnInit {
  private authService = inject(AuthService);

  user$ = this.authService.user;
  
  // Profile update state
  profileData: UpdateProfileRequest = { name: '', phone: '' };
  isUpdatingProfile = false;
  profileSuccess = '';
  profileError = '';
  profileFieldErrors: Record<'name' | 'phone', string> = {
    name: '',
    phone: ''
  };

  ngOnInit(): void {
    const user = this.user$();
    if (user) {
      this.profileData = { name: user.name || '', phone: user.phone || '' };
    }
  }

  formatRole(role: string | null | undefined): string {
    if (!role) {
      return '';
    }

    return role.charAt(0).toUpperCase() + role.slice(1).toLowerCase();
  }

  onUpdateProfile(): void {
    this.profileError = '';
    this.clearProfileFieldErrors();

    const name = this.profileData.name.trim();
    const phone = this.profileData.phone.trim();

    if (!name) {
      this.profileFieldErrors.name = 'Full name is required.';
    } else if (!/^[A-Za-z][A-Za-z\s.'-]{1,99}$/.test(name)) {
      this.profileFieldErrors.name = 'Please enter a valid full name.';
    }

    if (!phone) {
      this.profileFieldErrors.phone = 'Phone number is required.';
    } else if (!/^\d{10}$/.test(phone)) {
      this.profileFieldErrors.phone = 'Phone number must be exactly 10 digits.';
    }

    if (Object.values(this.profileFieldErrors).some(message => !!message)) {
      return;
    }

    this.isUpdatingProfile = true;
    this.profileError = '';
    this.profileSuccess = '';

    this.authService.updateProfile(this.profileData).pipe(
      catchError(err => {
        this.profileError = err.error?.message || 'Failed to update profile.';
        return of(null);
      }),
      finalize(() => this.isUpdatingProfile = false)
    ).subscribe((result: any) => {
      if (result !== null) {
        this.profileSuccess = 'Profile updated successfully. Please log in again to see changes.';
        // Ideally we would update the local signal, but for now we just show success
        setTimeout(() => this.profileSuccess = '', 5000);
      }
    });
  }

  onProfileFieldChange(field: 'name' | 'phone'): void {
    this.profileError = '';
    if (field === 'phone') {
      this.profileData.phone = (this.profileData.phone || '').replace(/\D/g, '').slice(0, 10);
    }
    this.profileFieldErrors[field] = '';
  }

  private clearProfileFieldErrors(): void {
    this.profileFieldErrors = {
      name: '',
      phone: ''
    };
  }
}
