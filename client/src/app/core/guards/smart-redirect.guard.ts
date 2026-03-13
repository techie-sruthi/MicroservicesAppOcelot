import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../shared/services/auth.service';

/**
 * Smart redirect guard for unknown routes
 * If user is logged in: redirect to appropriate products page based on role
 * If user is not logged in: redirect to login page
 */
export const smartRedirectGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Check if user is authenticated
  if (authService.isAuthenticated()) {
    const role = authService.getUserRole();
    
    
    // Redirect based on role
    if (role === 'Admin') {
      router.navigate(['/admin/products']);
    } else {
      router.navigate(['/user/products']);
    }
  } else {
    router.navigate(['/login']);
  }

  return false; 
};
