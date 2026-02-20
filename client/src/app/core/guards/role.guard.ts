import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (
  route: ActivatedRouteSnapshot
) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }

  const expectedRole = route.data['role'];
  const userRole = authService.getUserRole();

  if (userRole !== expectedRole) {
    // Redirect to appropriate dashboard based on actual role
    if (userRole === 'Admin') {
      router.navigate(['/admin/products']);
    } else if (userRole === 'User') {
      router.navigate(['/user/products']);
    } else {
      router.navigate(['/login']);
    }
    return false;
  }

  return true;
};
