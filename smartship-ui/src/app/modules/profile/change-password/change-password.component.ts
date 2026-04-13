import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ChangePasswordRequest } from '../../../shared/models/user.model';
import { catchError, finalize, timeout } from 'rxjs/operators';
import { EMPTY } from 'rxjs';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.css'
})
/**
 * Change-password screen for authenticated users.
 * Validates input locally and calls the identity API to update the password.
 */
export class ChangePasswordComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  // Password update state
  passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };
  isUpdatingPassword = signal(false);
  passwordSuccess = signal('');
  passwordError = signal('');
  passwordFieldErrors: Record<'currentPassword' | 'newPassword' | 'confirmPassword', string> = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  onChangePassword(): void {
    this.passwordError.set('');
    this.clearPasswordFieldErrors();

    if (!this.passwordData.currentPassword) {
      this.passwordFieldErrors.currentPassword = 'Current password is required.';
    }

    if (!this.passwordData.newPassword) {
      this.passwordFieldErrors.newPassword = 'New password is required.';
    }

    if (!this.passwordData.confirmPassword) {
      this.passwordFieldErrors.confirmPassword = 'Confirm new password is required.';
    }

    if (Object.values(this.passwordFieldErrors).some(message => !!message)) {
      return;
    }

    if (this.passwordData.newPassword.length < 8) {
      this.passwordFieldErrors.newPassword = 'New password must be at least 8 characters.';
      return;
    }

    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      this.passwordFieldErrors.confirmPassword = 'Passwords do not match.';
      return;
    }

    this.isUpdatingPassword.set(true);
    this.passwordError.set('');
    this.passwordSuccess.set('');

    const payload: ChangePasswordRequest = {
      oldPassword: this.passwordData.currentPassword,
      newPassword: this.passwordData.newPassword
    };

    this.authService.changePassword(payload).pipe(
      timeout(15000),
      catchError(err => {
        if (err?.name === 'TimeoutError') {
          this.passwordError.set('Request timed out. Please try again.');
          return EMPTY;
        }

        this.passwordError.set(
          err?.error?.message
          || err?.error?.title
          || err?.error?.detail
          || 'Failed to change password.'
        );
        return EMPTY;
      }),
      finalize(() => this.isUpdatingPassword.set(false))
    ).subscribe(() => {
      this.passwordSuccess.set('Password changed successfully. Redirecting to profile...');
      this.passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };
      this.clearPasswordFieldErrors();
      setTimeout(() => {
        this.passwordSuccess.set('');
        this.router.navigate(['/profile']);
      }, 1500);
    });
  }

  onPasswordFieldChange(field: 'currentPassword' | 'newPassword' | 'confirmPassword'): void {
    this.passwordError.set('');
    this.passwordFieldErrors[field] = '';
  }

  private clearPasswordFieldErrors(): void {
    this.passwordFieldErrors = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    };
  }
}
