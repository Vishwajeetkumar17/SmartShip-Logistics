/**
 * Authentication/identity API client.
 * Manages tokens + current user state and provides admin user/role management helpers via the gateway.
 */
import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  User, LoginRequest, SignupRequest, AuthResponse,
  UpdateProfileRequest, ChangePasswordRequest, RefreshTokenRequest,
  UserListItem, CreateUserRequest, UpdateUserRequest, Role, CreateRoleRequest, GoogleSignupRequest, VerifySignupOtpRequest
} from '../../shared/models/user.model';
import { PaginatedResponse } from '../../shared/models/pagination.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiBaseUrl}/identity/auth`;
  private currentUser = signal<User | null>(this.loadUserFromStorage());

  readonly user = this.currentUser.asReadonly();
  readonly isLoggedIn = computed(() => !!this.currentUser());

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  signup(data: SignupRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/signup`, data);
  }

  verifySignupOtp(data: VerifySignupOtpRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/signup/verify-otp`, data).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  googleSignup(data: GoogleSignupRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/google-signup`, data).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  logout(): void {
    const token = this.getToken();
    if (token) {
      this.http.post(`${this.apiUrl}/logout`, {}).subscribe();
    }
    this.clearAuthState();
    this.router.navigate(['/auth/login']);
  }

  clearAuthState(): void {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }

  refreshToken(): Observable<AuthResponse> {
    const refreshToken = localStorage.getItem('refresh_token') || '';
    const body: RefreshTokenRequest = { refreshToken };
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh-token`, body).pipe(
      tap(response => this.handleAuthResponse(response))
    );
  }

  getProfile(): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/profile`);
  }

  updateProfile(data: UpdateProfileRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/profile`, data);
  }

  changePassword(data: ChangePasswordRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/change-password`, data);
  }

  forgotPassword(data: { email: string }): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/forgot-password`, data);
  }

  resetPassword(data: any): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/reset-password`, data);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) {
      return false;
    }

    if (this.isTokenExpired(token)) {
      this.clearAuthState();
      return false;
    }

    return true;
  }

  getToken(): string | null {
    return localStorage.getItem('auth_token');
  }

  getUserRole(): string | null {
    const userRole = this.currentUser()?.role;
    if (userRole) {
      return userRole.toUpperCase();
    }

    const tokenRole = this.getRoleFromToken();
    return tokenRole ? tokenRole.toUpperCase() : null;
  }

  private handleAuthResponse(response: AuthResponse): void {
    localStorage.setItem('auth_token', response.token);
    localStorage.setItem('refresh_token', response.refreshToken);
    const user: User = {
      userId: response.userId,
      name: response.name,
      email: response.email,
      phone: response.phone,
      role: response.role
    };
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  private loadUserFromStorage(): User | null {
    const userJson = localStorage.getItem('user');
    if (userJson) {
      try {
        return JSON.parse(userJson) as User;
      } catch {
        return null;
      }
    }
    return null;
  }

  private isTokenExpired(token: string): boolean {
    const payload = this.parseJwtPayload(token);
    const exp = payload?.['exp'];
    if (typeof exp !== 'number') {
      return false;
    }

    return Date.now() >= exp * 1000;
  }

  private getRoleFromToken(): string | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    const payload = this.parseJwtPayload(token);
    if (!payload) {
      return null;
    }

    const directRole = payload['role'] ?? payload['Role'];
    if (typeof directRole === 'string' && directRole.trim().length > 0) {
      return directRole;
    }

    const claimRole = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (typeof claimRole === 'string' && claimRole.trim().length > 0) {
      return claimRole;
    }

    if (Array.isArray(claimRole)) {
      const firstRole = claimRole.find(value => typeof value === 'string' && value.trim().length > 0);
      return typeof firstRole === 'string' ? firstRole : null;
    }

    return null;
  }

  private parseJwtPayload(token: string): Record<string, unknown> | null {
    try {
      const tokenParts = token.split('.');
      if (tokenParts.length < 2 || !tokenParts[1]) {
        return null;
      }

      const base64 = tokenParts[1]
        .replace(/-/g, '+')
        .replace(/_/g, '/');
      const paddedBase64 = base64 + '='.repeat((4 - (base64.length % 4)) % 4);
      const decoded = atob(paddedBase64);
      return JSON.parse(decoded) as Record<string, unknown>;
    } catch {
      return null;
    }
  }

  // === Admin User Management ===

  getUsers(): Observable<UserListItem[]> {
    return this.getUsersPage(1, 5).pipe(map((response) => response.data));
  }

  getUsersPage(pageNumber: number, pageSize: number): Observable<PaginatedResponse<UserListItem>> {
    return this.http.get<unknown>(`${this.apiUrl}/users`, {
      params: {
        pageNumber: String(pageNumber),
        pageSize: String(pageSize)
      }
    }).pipe(
      map((payload) => this.normalizePaginatedResponse<UserListItem>(payload, pageNumber, pageSize))
    );
  }

  getUserById(id: number): Observable<UserListItem> {
    return this.http.get<UserListItem>(`${this.apiUrl}/users/${id}`);
  }

  createUser(data: CreateUserRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/users`, data);
  }

  updateUser(id: number, data: UpdateUserRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/users/${id}`, data);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/users/${id}`);
  }

  resendWelcomeEmail(id: number): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/users/${id}/resend-welcome`, {});
  }

  getRoles(): Observable<Role[]> {
    return this.getRolesPage(1, 50).pipe(
      map((response) => response.data)
    );
  }

  getRolesPage(pageNumber: number, pageSize: number): Observable<PaginatedResponse<Role>> {
    return this.http.get<unknown>(`${this.apiUrl}/roles`, {
      params: {
        pageNumber: String(pageNumber),
        pageSize: String(pageSize)
      }
    }).pipe(
      map((payload) => this.normalizePaginatedResponse<Role>(payload, pageNumber, pageSize)),
      map((response) => ({
        ...response,
        data: response.data.map((role) => ({
        ...role,
        name: role.name ?? role.roleName ?? ''
      }))
      }))
    );
  }

  createRole(data: CreateRoleRequest): Observable<Role> {
    return this.http.post<Role>(`${this.apiUrl}/roles`, data);
  }

  assignRole(userId: number, roleId: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/users/${userId}/role`, { roleId });
  }

  private normalizePaginatedResponse<T>(payload: unknown, pageNumber: number, pageSize: number): PaginatedResponse<T> {
    const normalizedPageNumber = Math.max(1, pageNumber);
    const normalizedPageSize = Math.max(1, pageSize);

    if (Array.isArray(payload)) {
      const startIndex = (normalizedPageNumber - 1) * normalizedPageSize;
      const items = payload.length > normalizedPageSize
        ? payload.slice(startIndex, startIndex + normalizedPageSize)
        : payload;

      return {
        data: items as T[],
        pageNumber: normalizedPageNumber,
        pageSize: normalizedPageSize,
        totalItems: payload.length,
        totalPages: Math.ceil(payload.length / normalizedPageSize),
        hasNextPage: false,
        hasPreviousPage: normalizedPageNumber > 1
      };
    }

    if (!payload || typeof payload !== 'object') {
      return {
        data: [],
        pageNumber: normalizedPageNumber,
        pageSize: normalizedPageSize,
        totalItems: 0,
        totalPages: 0,
        hasNextPage: false,
        hasPreviousPage: false
      };
    }

    const result = payload as Record<string, unknown>;
    const currentPage = Number(result['pageNumber'] ?? normalizedPageNumber);
    const currentPageSize = Number(result['pageSize'] ?? normalizedPageSize);
    const data = Array.isArray(result['data']) ? (result['data'] as T[]) : [];
    const startIndex = (Math.max(1, currentPage) - 1) * Math.max(1, currentPageSize);
    const items = data.length > Math.max(1, currentPageSize)
      ? data.slice(startIndex, startIndex + Math.max(1, currentPageSize))
      : data;
    const totalItems = Number(result['totalItems'] ?? data.length);
    const totalPages = Number(result['totalPages'] ?? Math.ceil(totalItems / Math.max(currentPageSize, 1)));

    return {
      data: items,
      pageNumber: Math.max(1, currentPage),
      pageSize: Math.max(1, currentPageSize),
      totalItems,
      totalPages,
      hasNextPage: Boolean(result['hasNextPage'] ?? currentPage < totalPages),
      hasPreviousPage: Boolean(result['hasPreviousPage'] ?? currentPage > 1)
    };
  }
}
