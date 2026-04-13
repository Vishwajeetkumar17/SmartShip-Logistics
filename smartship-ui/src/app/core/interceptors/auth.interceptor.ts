/**
 * HTTP interceptor that attaches the access token and transparently refreshes it on 401 responses.
 * Uses a single-flight refresh mechanism so concurrent 401s wait for the same refresh request.
 */
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, filter, ReplaySubject, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

let isRefreshingToken = false;
const refreshTokenSubject = new ReplaySubject<string | null>(1);

/**
 * Endpoints that should not trigger refresh-on-401 retry logic.
 * (Avoids refresh loops and allows auth pages to handle failures explicitly.)
 */
const isAuthEndpoint = (url: string): boolean =>
  url.includes('/identity/auth/login') ||
  url.includes('/identity/auth/signup') ||
  url.includes('/identity/auth/google-signup') ||
  url.includes('/identity/auth/refresh-token') ||
  url.includes('/identity/auth/forgot-password') ||
  url.includes('/identity/auth/reset-password');

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authService = inject(AuthService);
  const token = authService.getToken();

  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401 || isAuthEndpoint(req.url)) {
        return throwError(() => error);
      }

      const refreshToken = localStorage.getItem('refresh_token');
      if (!refreshToken) {
        authService.clearAuthState();
        router.navigate(['/auth/login']);
        return throwError(() => error);
      }

      if (isRefreshingToken) {
        return refreshTokenSubject.pipe(
          filter((newToken): newToken is string => !!newToken),
          take(1),
          switchMap((newToken) => {
            const retriedReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${newToken}`
              }
            });
            return next(retriedReq);
          })
        );
      }

      isRefreshingToken = true;
      refreshTokenSubject.next(null);

      return authService.refreshToken().pipe(
        switchMap((response) => {
          isRefreshingToken = false;
          refreshTokenSubject.next(response.token);

          const retriedReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${response.token}`
            }
          });

          return next(retriedReq);
        }),
        catchError((refreshError) => {
          isRefreshingToken = false;
          authService.clearAuthState();
          router.navigate(['/auth/login']);
          return throwError(() => refreshError);
        })
      );
    })
  );
};
