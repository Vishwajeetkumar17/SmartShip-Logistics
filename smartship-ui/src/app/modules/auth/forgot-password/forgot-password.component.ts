import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ForgotPasswordRequest } from '../../../shared/models/user.model';
import { catchError, finalize, timeout } from 'rxjs/operators';
import { EMPTY } from 'rxjs';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
/**
 * Password recovery flow.
 * Phase 1 requests a reset OTP; Phase 2 submits OTP + new password and then redirects to login.
 */
export class ForgotPasswordComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  isSubmitting = signal(false);
  successMessage = signal('');
  formError = signal('');
  fieldErrors: Record<'email' | 'otp' | 'newPassword' | 'confirmPassword', string> = {
    email: '',
    otp: '',
    newPassword: '',
    confirmPassword: ''
  };
  
  // Phase 1: Request Token
  // Phase 2: Enter Token & New Password
  phase = signal<1 | 2>(1);

  resetData = {
    otp: '',
    newPassword: '',
    confirmPassword: ''
  };

  isResetComplete = signal(false);

  onRequestReset(): void {
    this.clearFieldErrors();
    this.formError.set('');

    if (!this.email.trim()) {
      this.fieldErrors.email = 'Email address is required.';
      return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email.trim())) {
      this.fieldErrors.email = 'Please enter a valid email address.';
      return;
    }

    this.isSubmitting.set(true);
    this.successMessage.set('');

    const payload: ForgotPasswordRequest = { email: this.email };

    this.authService.forgotPassword(payload).pipe(
      timeout(15000),
      catchError(err => {
        if (err?.name === 'TimeoutError') {
          this.formError.set('Request timed out. Please check if the server is running and try again.');
          return EMPTY;
        }
        this.formError.set(this.getErrorMessage(err, 'Failed to send OTP. Please try again.'));
        return EMPTY;
      }),
      finalize(() => this.isSubmitting.set(false))
    ).subscribe(() => {
      this.successMessage.set('A 6-digit OTP has been sent to your email.');
      this.phase.set(2);
      this.isResetComplete.set(false);
    });
  }

  onResetPassword(): void {
    this.clearFieldErrors();
    this.formError.set('');

    if (!this.resetData.otp.trim()) {
      this.fieldErrors.otp = 'Reset OTP is required.';
    }

    if (!this.resetData.newPassword) {
      this.fieldErrors.newPassword = 'New password is required.';
    }

    if (!this.resetData.confirmPassword) {
      this.fieldErrors.confirmPassword = 'Confirm password is required.';
    }

    if (Object.values(this.fieldErrors).some(message => !!message)) {
      return;
    }

    if (!/^\d{6}$/.test(this.resetData.otp)) {
      this.fieldErrors.otp = 'Please enter a valid 6-digit OTP.';
      return;
    }

    if (this.resetData.newPassword !== this.resetData.confirmPassword) {
      this.fieldErrors.confirmPassword = 'Passwords do not match.';
      return;
    }

    if (this.resetData.newPassword.length < 8) {
      this.fieldErrors.newPassword = 'New password must be at least 8 characters.';
      return;
    }

    this.isSubmitting.set(true);

    const payload = {
      email: this.email,
      token: this.resetData.otp,
      newPassword: this.resetData.newPassword
    };

    this.authService.resetPassword(payload).pipe(
      timeout(15000),
      catchError(err => {
        if (err?.name === 'TimeoutError') {
          this.formError.set('Request timed out. Please try again.');
          return EMPTY;
        }
        this.formError.set(this.getErrorMessage(err, 'Invalid OTP or failed to reset password.'));
        return EMPTY;
      }),
      finalize(() => this.isSubmitting.set(false))
    ).subscribe(() => {
      this.successMessage.set('Password has been reset successfully. Redirecting to login...');
      this.isResetComplete.set(true);
      setTimeout(() => {
        this.router.navigate(['/auth/login']);
      }, 3000);
    });
  }

  onFieldChange(field: 'email' | 'otp' | 'newPassword' | 'confirmPassword'): void {
    this.formError.set('');
    this.fieldErrors[field] = '';
  }

  private clearFieldErrors(): void {
    this.fieldErrors = {
      email: '',
      otp: '',
      newPassword: '',
      confirmPassword: ''
    };
  }

  private getErrorMessage(error: any, fallback: string): string {
    return error?.error?.message
      || error?.error?.title
      || error?.error?.detail
      || fallback;
  }
}
