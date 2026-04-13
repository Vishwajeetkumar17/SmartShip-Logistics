/**
 * Shared identity/auth models used by the Angular client.
 * These interfaces intentionally mirror backend DTO shapes (IdentityService) for request/response typing.
 */

// Matches backend: SmartShip.IdentityService.DTOs

export interface LoginRequest {
  email: string;
  password: string;
}

export interface SignupRequest {
  name: string;
  email: string;
  phone: string;
  password: string;
  roleId: number;
}

export interface VerifySignupOtpRequest {
  email: string;
  otp: string;
}

export interface GoogleSignupRequest {
  idToken: string;
}

// Matches AuthDTO from backend
export interface AuthResponse {
  userId: number;
  name: string;
  email: string;
  phone: string;
  token: string;
  refreshToken: string;
  role: string;
}

export interface User {
  userId: number;
  name: string;
  email: string;
  phone: string;
  role: string;
}

export interface UpdateProfileRequest {
  name: string;
  phone: string;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}

// Admin User Management DTOs
export interface CreateUserRequest {
  name: string;
  email: string;
  phone: string;
  password: string;
  roleId: number;
}

export interface UpdateUserRequest {
  name: string;
  email: string;
  phone: string;
}

export interface UserListItem {
  userId: number;
  name: string;
  email: string;
  phone: string;
  role: string;
}

export interface Role {
  roleId: number;
  name?: string;
  roleName?: string;
}

export interface CreateRoleRequest {
  name: string;
}

export interface AssignRoleRequest {
  roleId: number;
}
