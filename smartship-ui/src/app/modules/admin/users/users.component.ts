import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { UserListItem, CreateUserRequest, UpdateUserRequest, Role } from '../../../shared/models/user.model';
import { catchError, finalize } from 'rxjs/operators';
import { EMPTY, of } from 'rxjs';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
/**
 * Admin user management screen (via IdentityService through the gateway).
 * Supports paginated user listing, create/edit/delete, and role assignment with safeguards for protected/self accounts.
 */
export class UsersComponent implements OnInit {
  private authService = inject(AuthService);
  private readonly protectedUserIds = new Set<number>([1005]);
  private readonly defaultPageSize = 5;
  private readonly _users = signal<UserListItem[]>([]);
  private readonly _roles = signal<Role[]>([]);
  private readonly _isLoading = signal(true);
  private readonly _error = signal('');
  private readonly _success = signal('');
  private readonly _showCreateForm = signal(false);
  private readonly _isSubmitting = signal(false);
  private readonly _createFormError = signal('');
  private readonly _editingUserId = signal<number | null>(null);
  private readonly _assigningRoleUserId = signal<number | null>(null);
  private readonly _pageNumber = signal(1);
  private readonly _pageSize = signal(this.defaultPageSize);
  private readonly _totalItems = signal(0);
  private readonly _totalPages = signal(0);

  get users(): UserListItem[] {
    return this._users();
  }

  get roles(): Role[] {
    return this._roles();
  }

  get isLoading(): boolean {
    return this._isLoading();
  }

  get error(): string {
    return this._error();
  }

  get success(): string {
    return this._success();
  }

  get pageNumber(): number {
    return this._pageNumber();
  }

  get totalPages(): number {
    return this._totalPages();
  }

  get totalItems(): number {
    return this._totalItems();
  }

  get hasPreviousPage(): boolean {
    return this.pageNumber > 1;
  }

  get hasNextPage(): boolean {
    return this.pageNumber < this.totalPages;
  }

  // Create user form
  get showCreateForm(): boolean {
    return this._showCreateForm();
  }

  newUser: CreateUserRequest = { name: '', email: '', phone: '', password: '', roleId: 2 };
  get isSubmitting(): boolean {
    return this._isSubmitting();
  }

  get createFormError(): string {
    return this._createFormError();
  }
  createFieldErrors: Record<'name' | 'email' | 'phone' | 'password' | 'roleId', string> = {
    name: '',
    email: '',
    phone: '',
    password: '',
    roleId: ''
  };

  // Edit user
  get editingUserId(): number | null {
    return this._editingUserId();
  }

  editData: UpdateUserRequest = { name: '', email: '', phone: '' };

  // Role assignment
  get assigningRoleUserId(): number | null {
    return this._assigningRoleUserId();
  }

  get currentUserId(): number | null {
    return this.authService.user()?.userId ?? null;
  }

  selectedRoleId: number = 0;

  isProtectedAccount(userId: number): boolean {
    return this.protectedUserIds.has(userId);
  }

  private isValidName(name: string): boolean {
    return /^[A-Za-z][A-Za-z\s.'-]{1,99}$/.test((name || '').trim());
  }

  private isValidPhone(phone: string): boolean {
    const normalized = (phone || '').trim();
    return /^\d{10}$/.test(normalized);
  }

  private isValidEmail(email: string): boolean {
    const normalized = (email || '').trim();
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(normalized) && normalized.length <= 150;
  }

  private isValidPassword(password: string): boolean {
    return /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,128}$/.test(password || '');
  }

  isCreateUserValid(): boolean {
    return this.isValidName(this.newUser.name)
      && !!this.newUser.email?.trim()
      && this.isValidPhone(this.newUser.phone)
      && this.isValidPassword(this.newUser.password)
      && !!this.newUser.roleId;
  }

  ngOnInit(): void {
    this.loadUsers();
    this.loadRoles();
  }

  loadUsers(pageNumber: number = 1): void {
    this._isLoading.set(true);
    this._error.set('');
    this.authService.getUsersPage(pageNumber, this._pageSize()).pipe(
      catchError(err => {
        this._error.set(err.status === 0
          ? 'Cannot connect to backend. Ensure the Gateway (http://localhost:5000) is running.'
          : err.error?.message || 'Failed to load users.');
        return of({
          data: [] as UserListItem[],
          pageNumber,
          pageSize: this._pageSize(),
          totalItems: 0,
          totalPages: 0,
          hasNextPage: false,
          hasPreviousPage: false
        });
      }),
      finalize(() => this._isLoading.set(false))
    ).subscribe(response => {
      this._users.set(response.data);
      this._pageNumber.set(response.pageNumber);
      this._totalItems.set(response.totalItems);
      this._totalPages.set(response.totalPages);
    });
  }

  goToPage(pageNumber: number): void {
    if (pageNumber < 1 || pageNumber > this.totalPages || pageNumber === this.pageNumber) {
      return;
    }
    this.loadUsers(pageNumber);
  }

  loadRoles(): void {
    this.authService.getRoles().pipe(
      catchError(() => of([]))
    ).subscribe(data => this._roles.set(data));
  }

  toggleCreateForm(): void {
    this._createFormError.set('');
    this.clearCreateFieldErrors();
    this._showCreateForm.set(!this._showCreateForm());
    if (!this._showCreateForm()) {
      this.newUser = { name: '', email: '', phone: '', password: '', roleId: 2 };
    }
  }

  submitCreateUser(): void {
    this._error.set('');
    this._createFormError.set('');
    this.clearCreateFieldErrors();

    if (!this.validateCreateUserForm()) {
      return;
    }

    this._isSubmitting.set(true);
    this.authService.createUser(this.newUser).pipe(
      catchError(err => {
        this._createFormError.set(err.error?.message || 'Failed to create user.');
        return EMPTY;
      }),
      finalize(() => this._isSubmitting.set(false))
    ).subscribe(() => {
      this._success.set('User created successfully!');
      this._showCreateForm.set(false);
      this.newUser = { name: '', email: '', phone: '', password: '', roleId: 2 };
      this.clearCreateFieldErrors();
      this.loadUsers();
      setTimeout(() => this._success.set(''), 3000);
    });
  }

  onCreateFieldChange(field: 'name' | 'email' | 'phone' | 'password' | 'roleId'): void {
    if (field === 'phone') {
      this.newUser.phone = (this.newUser.phone || '').replace(/\D/g, '').slice(0, 10);
    }
    this._createFormError.set('');
    this.createFieldErrors[field] = '';
  }

  private validateCreateUserForm(): boolean {
    const name = this.newUser.name?.trim() || '';
    const email = this.newUser.email?.trim() || '';
    const phone = this.newUser.phone?.trim() || '';
    const password = this.newUser.password || '';

    if (!name) {
      this.createFieldErrors.name = 'Full name is required.';
    } else if (!this.isValidName(name)) {
      this.createFieldErrors.name = 'Enter a valid full name (letters and spaces only).';
    }

    if (!email) {
      this.createFieldErrors.email = 'Email is required.';
    } else if (!this.isValidEmail(email)) {
      this.createFieldErrors.email = 'Enter a valid email address.';
    }

    if (!phone) {
      this.createFieldErrors.phone = 'Phone is required.';
    } else if (!this.isValidPhone(phone)) {
      this.createFieldErrors.phone = 'Enter a valid 10-digit mobile number.';
    }

    if (!password) {
      this.createFieldErrors.password = 'Password is required.';
    } else if (!this.isValidPassword(password)) {
      this.createFieldErrors.password = 'Password must be 8+ chars with uppercase, lowercase, number and special character.';
    }

    if (!this.newUser.roleId) {
      this.createFieldErrors.roleId = 'Please select a role.';
    }

    return !Object.values(this.createFieldErrors).some(message => !!message);
  }

  private clearCreateFieldErrors(): void {
    this.createFieldErrors = {
      name: '',
      email: '',
      phone: '',
      password: '',
      roleId: ''
    };
  }

  startEdit(user: UserListItem): void {
    this._editingUserId.set(user.userId);
    this.editData = { name: user.name, email: user.email, phone: user.phone };
  }

  cancelEdit(): void {
    this._editingUserId.set(null);
  }

  saveEdit(userId: number): void {
    this._error.set('');

    if (!this.editData.email) {
      const existingUser = this._users().find(u => u.userId === userId);
      this.editData.email = existingUser?.email ?? '';
    }

    this.authService.updateUser(userId, this.editData).pipe(
      catchError(err => {
        this._error.set(err.error?.message || err.error?.title || err.error?.detail || 'Failed to update user.');
        return EMPTY;
      })
    ).subscribe(() => {
      this._success.set('User updated!');
      this._editingUserId.set(null);
      this.loadUsers();
      setTimeout(() => this._success.set(''), 3000);
    });
  }

  deleteUser(userId: number): void {
    if (this.isProtectedAccount(userId)) {
      this._error.set('This protected account cannot be deleted.');
      return;
    }

    if (this.currentUserId === userId) {
      this._error.set('You cannot delete your own account.');
      return;
    }

    if (!confirm('Are you sure you want to delete this user?')) return;
    this.authService.deleteUser(userId).pipe(
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to delete user.');
        return EMPTY;
      })
    ).subscribe(() => {
      this._success.set('User deleted.');
      this.loadUsers();
      setTimeout(() => this._success.set(''), 3000);
    });
  }

  startAssignRole(user: UserListItem): void {
    if (this.isProtectedAccount(user.userId)) {
      this._error.set('This protected account role cannot be changed.');
      return;
    }

    if (this.currentUserId === user.userId) {
      this._error.set('You cannot change your own role.');
      return;
    }

    this._assigningRoleUserId.set(user.userId);
    const userRole = this.getUserRoleLabel(user).toLowerCase();
    const currentRole = this._roles().find(r => this.getRoleLabel(r).toLowerCase() === userRole);
    this.selectedRoleId = currentRole?.roleId || 0;
  }

  cancelAssignRole(): void {
    this._assigningRoleUserId.set(null);
  }

  saveAssignRole(userId: number): void {
    if (this.isProtectedAccount(userId)) {
      this._error.set('This protected account role cannot be changed.');
      this._assigningRoleUserId.set(null);
      return;
    }

    if (!this.selectedRoleId) return;
    this.authService.assignRole(userId, this.selectedRoleId).pipe(
      catchError(err => {
        this._error.set(err.error?.message || 'Failed to assign role.');
        return EMPTY;
      })
    ).subscribe(() => {
      this._success.set('Role assigned!');
      this._assigningRoleUserId.set(null);
      this.loadUsers();
      setTimeout(() => this._success.set(''), 3000);
    });
  }
  
  getRoleLabel(role: Role): string {
    return role.name ?? role.roleName ?? '';
  }

  getUserRoleLabel(user: UserListItem): string {
    const candidate = [
      user.role,
      (user as unknown as { roleName?: string }).roleName,
      (user as unknown as { roleLabel?: string }).roleLabel,
    ].find(value => typeof value === 'string' && value.trim().length > 0);

    if (typeof candidate === 'string') {
      return candidate.trim();
    }

    const roleId = Number((user as unknown as { roleId?: number }).roleId ?? 0);
    if (roleId > 0) {
      const matchedRole = this._roles().find(role => role.roleId === roleId);
      const matchedLabel = matchedRole ? this.getRoleLabel(matchedRole) : '';
      if (matchedLabel) {
        return matchedLabel;
      }
    }

    return 'Unknown';
  }

  getUserRoleClass(user: UserListItem): string {
    const normalizedRole = this.getUserRoleLabel(user)
      .toLowerCase()
      .replace(/[^a-z0-9]+/g, '');
    return `role-${normalizedRole || 'unknown'}`;
  }
}
