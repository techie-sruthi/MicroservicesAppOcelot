import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError, switchMap } from 'rxjs';
import { AuthService } from '../../shared/services/auth.service';
import { MessageService } from 'primeng/api';

let isRefreshing = false;

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const messageService = inject(MessageService);

  const token = authService.getAccessToken();

  // Add token if exists
  if (token) {
    req = addTokenToRequest(req, token);
  }

  return next(req).pipe(
    catchError((error) => {
      // If 401 and not already refreshing, try to refresh token
      if (error.status === 401 && !isRefreshing) {
        return handle401Error(req, next, authService, router);
      }

      // If 403 and not a change-password request, show forbidden toast and logout
      if (error.status === 403 && !req.url.includes('/ChangePassword')) {
        messageService.add({
          key: 'global',
          severity: 'error',
          summary: 'Access Denied',
          detail: 'You do not have permission to perform this action.',
          life: 3000
        });
        authService.logout();
        router.navigate(['/login']);
      }

      return throwError(() => error);
    })
  );
};

function addTokenToRequest(request: HttpRequest<any>, token: string): HttpRequest<any> {
  return request.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
}

function handle401Error(
  request: HttpRequest<any>,
  next: HttpHandlerFn,
  authService: AuthService,
  router: Router
) {

  if (request.url.includes('/refresh')) {
    authService.logout();
    router.navigate(['/login']);
    return throwError(() => new Error('Refresh token expired'));
  }

  isRefreshing = true;

  return authService.refreshToken().pipe(
    switchMap((response: any) => {
      isRefreshing = false;

      const clonedRequest = addTokenToRequest(request, response.accessToken);
      return next(clonedRequest);
    }),
    catchError((err) => {
      isRefreshing = false;

      authService.logout();
      router.navigate(['/login']);

      return throwError(() => err);
    })
  );
}
