import { ChangeDetectorRef, Component, inject, OnInit, AfterViewInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LoginRequest } from '../../../shared/models/user.model';
import { timeout, catchError, finalize } from 'rxjs/operators';
import { throwError } from 'rxjs';

import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
/**
 * Login screen with email/password authentication and Google Sign-In.
 * Handles role-aware routing, returnUrl redirects, and maps common backend auth errors to field-level messages.
 */
export class LoginComponent implements OnInit, AfterViewInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private cdr = inject(ChangeDetectorRef);

  credentials: LoginRequest = { email: '', password: '' };
  selectedRole = 'CUSTOMER';
  isLoading = false;
  formError = '';
  fieldErrors: Record<'email' | 'password', string> = {
    email: '',
    password: ''
  };

  private googleButtonRendered = false;

  private getDefaultDashboardRoute(role: string | null | undefined): string {
    return String(role ?? '').toUpperCase() === 'ADMIN' ? '/admin/dashboard' : '/customer/dashboard';
  }

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      const role = this.authService.getUserRole();
      this.router.navigate([this.getDefaultDashboardRoute(role)]);
    }
  }

  ngAfterViewInit(): void {
    this.initializeGoogleSignIn();
  }

  @HostListener('window:resize')
  onWindowResize(): void {
    if (this.googleButtonRendered) {
      this.initializeGoogleSignIn();
    }
  }

  onLogin(form: NgForm): void {
    this.isLoading = true;
    this.formError = '';
    this.clearFieldErrors();

    if (form.invalid) {
      this.isLoading = false;
      Object.values(form.controls).forEach(control => {
        control.markAsTouched();
      });

      if (!this.credentials.email.trim()) {
        this.fieldErrors.email = 'Email address is required.';
      }

      if (!this.credentials.password) {
        this.fieldErrors.password = 'Password is required.';
      }
      return;
    }

    this.authService.login(this.credentials).pipe(
      timeout(30000),
      catchError(err => {
        if (err.name === 'TimeoutError') {
          return throwError(() => ({ error: { message: 'Server is not responding. Please ensure the backend is running.' } }));
        }
        if (err.status === 0) {
          return throwError(() => ({ error: { message: 'Cannot connect to server. Please ensure the backend Gateway is running.' } }));
        }
        return throwError(() => err);
      }),
      finalize(() => { this.isLoading = false; this.cdr.detectChanges(); })
    ).subscribe({
      next: (response) => {
        if ((response.role || '').toUpperCase() !== (this.selectedRole || '').toUpperCase()) {
          this.authService.clearAuthState();
          this.fieldErrors.email = 'No account found with this email address for this role.';
          return;
        }

        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
        const role = response.role;
        if (returnUrl !== '/') {
          this.router.navigateByUrl(returnUrl);
        } else {
          this.router.navigate([this.getDefaultDashboardRoute(role)]);
        }
      },
      error: (err) => {
        const rawMessage = String(err?.error?.message || err?.error?.title || '').toLowerCase();
        if (rawMessage.includes("user doesn't exist") || rawMessage.includes('user does not exist') || rawMessage.includes('not found') || rawMessage.includes('invalid email')) {
          this.fieldErrors.email = 'No account found with this email address.';
          return;
        }

        if (rawMessage.includes('incorrect password') || rawMessage.includes('invalid password') || rawMessage.includes('wrong password') || rawMessage.includes('invalid credentials')) {
          this.fieldErrors.password = 'The password you entered is incorrect.';
          return;
        }

        this.formError = err.error?.message || err.error?.title || 'Login failed. Please try again.';
      }
    });
  }

  onFieldChange(field: 'email' | 'password'): void {
    this.formError = '';
    this.fieldErrors[field] = '';
  }

  private initializeGoogleSignIn(): void {
    if (!environment.googleClientId) {
      return;
    }

    const render = () => {
      // @ts-ignore
      const google = window.google;
      const container = document.getElementById('google-login-btn');

      if (!google || !container) {
        return;
      }

      container.innerHTML = '';

      const measuredWidth = Math.max(220, Math.floor(container.getBoundingClientRect().width));

      google.accounts.id.initialize({
        client_id: environment.googleClientId,
        callback: (response: any) => this.handleGoogleResponse(response)
      });

      google.accounts.id.renderButton(container, {
        theme: 'outline',
        size: 'large',
        text: 'signin_with',
        // Visually keeps the Google mark closer to the label.
        // (Google renders this into an iframe; CSS control is limited.)
        // @ts-ignore
        logo_alignment: 'center',
        width: measuredWidth
      });
      this.googleButtonRendered = true;
    };

    // @ts-ignore
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

  private handleGoogleResponse(response: any): void {
    if (response.credential) {
       this.isLoading = true;
       this.authService.googleSignup({ idToken: response.credential }).pipe(
         finalize(() => { this.isLoading = false; this.cdr.detectChanges(); })
       ).subscribe({
         next: (res) => {
           this.router.navigate([this.getDefaultDashboardRoute(res.role)]);
         },
         error: (err) => {
           this.formError = err.error?.message || err.error?.title || 'Google sign-in failed.';
         }
       });
    }
  }

  private clearFieldErrors(): void {
    this.fieldErrors = {
      email: '',
      password: ''
    };
  }
}
