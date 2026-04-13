import { ChangeDetectorRef, Component, HostListener, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { SignupRequest } from '../../../shared/models/user.model';
import { timeout, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';

type GoogleCredentialResponse = { credential: string };

type GoogleIdConfiguration = {
  client_id: string;
  callback: (response: GoogleCredentialResponse) => void;
  context?: string;
};

type GoogleButtonConfiguration = {
  theme?: 'outline' | 'filled_blue' | 'filled_black';
  size?: 'large' | 'medium' | 'small';
  text?: 'signup_with' | 'signin_with' | 'continue_with';
  shape?: 'rectangular' | 'pill' | 'circle' | 'square';
  width?: number;
};

type GoogleApi = {
  accounts: {
    id: {
      initialize: (config: GoogleIdConfiguration) => void;
      renderButton: (element: HTMLElement, options: GoogleButtonConfiguration) => void;
      prompt: (callback?: (notification: any) => void) => void;
    };
  };
};

declare global {
  interface Window {
    google?: GoogleApi;
  }
}

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './signup.component.html',
  styleUrl: './signup.component.css'
})
/**
 * Signup screen with OTP-based registration flow and optional Google sign-up.
 * Phase 1 requests an email OTP; Phase 2 verifies OTP and completes account creation.
 */
export class SignupComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  signupData: SignupRequest = {
    name: '',
    email: '',
    phone: '',
    password: '',
    roleId: 2
  };

  confirmPassword = '';
  otpCode = '';
  otpRequested = false;
  otpRequestedForEmail = '';
  isLoading = false;
  formError = '';
  infoMessage = '';
  fieldErrors: Record<'name' | 'email' | 'phone' | 'password' | 'confirmPassword' | 'otpCode', string> = {
    name: '',
    email: '',
    phone: '',
    password: '',
    confirmPassword: '',
    otpCode: ''
  };
  private googleButtonRendered = false;

  private readonly namePattern = /^[A-Za-z][A-Za-z\s.'-]{1,99}$/;
  private readonly emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  private readonly phonePattern = /^[0-9]{10}$/;
  private readonly passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^A-Za-z0-9]).{8,128}$/;

  ngAfterViewInit(): void {
    this.initializeGoogleSignup();
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    if (this.googleButtonRendered) {
      this.initializeGoogleSignup();
    }
  }

  onSignup(): void {
    if (this.isLoading) {
      return;
    }

    this.formError = '';
    this.infoMessage = '';
    this.clearFieldErrors();

    if (!this.validateForm()) {
      return;
    }

    const normalizedEmail = this.signupData.email.trim().toLowerCase();

    if (!this.otpRequested) {
      this.isLoading = true;

      this.authService.signup(this.signupData).pipe(
        timeout(10000),
        catchError(err => {
          if (err.name === 'TimeoutError') {
            return throwError(() => ({ error: { message: 'Server is not responding. Please ensure the backend is running.' } }));
          }
          if (err.status === 0) {
            return throwError(() => ({ error: { message: 'Cannot connect to server. Please ensure the backend Gateway is running.' } }));
          }
          return throwError(() => err);
        })
      ).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.otpRequested = true;
          this.otpRequestedForEmail = normalizedEmail;
          this.infoMessage = response?.message || 'OTP sent to your email. Please enter it below to complete signup.';
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.isLoading = false;
          this.formError = this.extractErrorMessage(err, 'Unable to send signup OTP. Please try again.');
          this.cdr.detectChanges();
        }
      });

      return;
    }

    if (this.otpRequestedForEmail !== normalizedEmail) {
      this.otpRequested = false;
      this.otpCode = '';
      this.fieldErrors.email = 'Email changed after OTP request. Click Create Account again to get a new OTP.';
      return;
    }

    const otp = this.otpCode.trim();

    this.isLoading = true;

    this.authService.verifySignupOtp({
      email: this.signupData.email,
      otp
    }).pipe(
      timeout(10000),
      catchError(err => {
        if (err.name === 'TimeoutError') {
          return throwError(() => ({ error: { message: 'Server is not responding. Please ensure the backend is running.' } }));
        }
        if (err.status === 0) {
          return throwError(() => ({ error: { message: 'Cannot connect to server. Please ensure the backend Gateway is running.' } }));
        }
        return throwError(() => err);
      })
    ).subscribe({
      next: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
        this.router.navigate(['/customer/dashboard']);
      },
      error: (err) => {
        this.isLoading = false;
        this.formError = this.extractErrorMessage(err, 'Registration failed. Please try again.');
        this.cdr.detectChanges();
      }
    });
  }

  onResendOtp(): void {
    if (this.isLoading) {
      return;
    }

    this.otpRequested = false;
    this.otpCode = '';
    this.fieldErrors.otpCode = '';
    this.onSignup();
  }

  onFieldChange(field: 'name' | 'email' | 'phone' | 'password' | 'confirmPassword' | 'otpCode'): void {
    this.formError = '';
    this.fieldErrors[field] = '';
  }

  private clearFieldErrors(): void {
    this.fieldErrors = {
      name: '',
      email: '',
      phone: '',
      password: '',
      confirmPassword: '',
      otpCode: ''
    };
  }

  private validateForm(): boolean {
    const name = this.signupData.name.trim();
    const email = this.signupData.email.trim();
    const phone = this.signupData.phone.trim();
    const password = this.signupData.password;
    const confirmPassword = this.confirmPassword;

    if (!name) {
      this.fieldErrors.name = 'Full name is required.';
    }

    if (!email) {
      this.fieldErrors.email = 'Email address is required.';
    }

    if (!phone) {
      this.fieldErrors.phone = 'Phone number is required.';
    }

    if (!password) {
      this.fieldErrors.password = 'Password is required.';
    }

    if (!confirmPassword) {
      this.fieldErrors.confirmPassword = 'Confirm password is required.';
    }

    if (this.otpRequested && !this.otpCode.trim()) {
      this.fieldErrors.otpCode = 'Email OTP is required.';
    }

    if (Object.values(this.fieldErrors).some(message => !!message)) {
      return false;
    }

    if (!this.namePattern.test(name)) {
      this.fieldErrors.name = 'Please enter a valid full name.';
    }

    if (!this.emailPattern.test(email)) {
      this.fieldErrors.email = 'Please enter a valid email address.';
    }

    if (!this.phonePattern.test(phone)) {
      this.fieldErrors.phone = 'Phone number must be exactly 10 digits.';
    }

    if (password.length < 8) {
      this.fieldErrors.password = 'Password must be at least 8 characters long.';
    } else if (!this.passwordPattern.test(password)) {
      this.fieldErrors.password = 'Password must include uppercase, lowercase, number, and special character.';
    }

    if (password !== confirmPassword) {
      this.fieldErrors.confirmPassword = 'Passwords do not match.';
    }

    if (this.otpRequested && !/^[0-9]{6}$/.test(this.otpCode.trim())) {
      this.fieldErrors.otpCode = 'Please enter a valid 6-digit OTP.';
    }

    return !Object.values(this.fieldErrors).some(message => !!message);
  }

  private initializeGoogleSignup(): void {
    if (!environment.googleClientId) {
      return;
    }

    const render = () => {
      const google = window.google;
      const container = document.getElementById('google-signup-btn');

      if (!google || !container) {
        return;
      }

      container.innerHTML = '';

      const measuredWidth = Math.max(220, Math.floor(container.getBoundingClientRect().width));

      google.accounts.id.initialize({
        client_id: environment.googleClientId,
        callback: (response: GoogleCredentialResponse) => this.onGoogleCredential(response)
      });

      google.accounts.id.renderButton(container, {
        theme: 'outline',
        size: 'large',
        text: 'signup_with',
        // Visually keeps the Google mark closer to the label.
        // (Google renders this into an iframe; CSS control is limited.)
        // @ts-ignore
        logo_alignment: 'center',
        width: measuredWidth
      });

      this.googleButtonRendered = true;
    };

    if (window.google) {
      render();
      return;
    }

    const existingScript = document.querySelector('script[src="https://accounts.google.com/gsi/client"]');
    if (existingScript) {
      existingScript.addEventListener('load', render, { once: true });
      return;
    }

    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    script.onload = render;
    document.head.appendChild(script);
  }

  private onGoogleCredential(response: GoogleCredentialResponse): void {
    if (!response?.credential) {
      this.formError = 'Google sign-up failed. No credential was returned.';
      this.cdr.detectChanges();
      return;
    }

    this.isLoading = true;
    this.formError = '';

    this.authService.googleSignup({ idToken: response.credential }).pipe(
      timeout(10000),
      catchError(err => {
        if (err.name === 'TimeoutError') {
          return throwError(() => ({ error: { message: 'Google sign-up timed out. Please try again.' } }));
        }
        if (err.status === 0) {
          return throwError(() => ({ error: { message: 'Cannot connect to server. Please ensure the backend Gateway is running.' } }));
        }
        return throwError(() => err);
      })
    ).subscribe({
      next: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
        this.router.navigate(['/customer/dashboard']);
      },
      error: (err) => {
        this.isLoading = false;
        this.formError = this.extractErrorMessage(err, 'Google registration failed. Please try again.');
        this.cdr.detectChanges();
      }
    });
  }

  private extractErrorMessage(err: any, fallback: string): string {
    const payload = err?.error;

    if (payload?.message) {
      return payload.message;
    }

    if (payload?.title) {
      return payload.title;
    }

    if (typeof payload === 'string' && payload.trim().length > 0) {
      try {
        const parsed = JSON.parse(payload);
        return parsed?.message || parsed?.title || fallback;
      } catch {
        return payload;
      }
    }

    if (err?.message) {
      return err.message;
    }

    return fallback;
  }
}
