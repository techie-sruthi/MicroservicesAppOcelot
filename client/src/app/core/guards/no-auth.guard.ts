import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../shared/services/auth.service';

export const noAuthGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    const role = authService.getUserRole();

    if (role === 'Admin') {
      router.navigate(['/admin/products']);
    } else {
      router.navigate(['/user/products']);
    }
    return false;
  }

  return true;
};
