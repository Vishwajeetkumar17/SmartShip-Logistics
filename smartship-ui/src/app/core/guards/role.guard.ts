/**
 * Route guard that enforces role-based access using `route.data.roles`.
 * If unauthenticated, redirects to login with `returnUrl`; if unauthorized, redirects to a role-appropriate dashboard.
 */
import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const expectedRoles = ((route.data?.['roles'] as string[] | undefined) ?? []).map(role => role.toUpperCase());

  if (expectedRoles.length === 0) {
    return true;
  }

  const userRole = authService.getUserRole()?.toUpperCase();

  if (!userRole) {
    authService.clearAuthState();
    router.navigate(['/auth/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }

  if (expectedRoles.includes(userRole)) {
    return true;
  }

  const fallbackRoute = userRole === 'ADMIN' ? '/admin/dashboard' : '/customer/dashboard';
  router.navigate([fallbackRoute]);
  return false;
};
