import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { smartRedirectGuard } from './core/guards/smart-redirect.guard';
import { noAuthGuard } from './core/guards/no-auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  {
    path: 'login',
    canActivate: [noAuthGuard],
    loadComponent: () => import('./features/auth/components/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    canActivate: [noAuthGuard],
    loadComponent: () => import('./features/auth/components/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'forgot-password',
    canActivate: [noAuthGuard],
    loadComponent: () => import('./features/auth/components/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password',
    canActivate: [noAuthGuard],
    loadComponent: () => import('./features/auth/components/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },

  {
    path: 'admin/products',
    canActivate: [authGuard, roleGuard],
    data: { role: 'Admin' },
    loadComponent: () => import('./features/admin/components/products/admin-products.component').then(m => m.AdminProductsComponent)
  },
  {
    path: 'admin/users',
    canActivate: [authGuard, roleGuard],
    data: { role: 'Admin' },
    loadComponent: () => import('./features/admin/components/users/admin-users.component').then(m => m.AdminUsersComponent)
  },
  {
    path: 'admin/change-password',
    canActivate: [authGuard, roleGuard],
    data: { role: 'Admin' },
    loadComponent: () => import('./shared/components/change-password/change-password.component').then(m => m.ChangePasswordComponent)
  },

  {
    path: 'user/products',
    canActivate: [authGuard, roleGuard],
    data: { role: 'User' },
    loadComponent: () => import('./features/user/components/products/user-products.component').then(m => m.UserProductsComponent)
  },
  {
    path: 'user/change-password',
    canActivate: [authGuard, roleGuard],
    data: { role: 'User' },
    loadComponent: () => import('./shared/components/change-password/change-password.component').then(m => m.ChangePasswordComponent)
  },

  { 
    path: '**',
    canActivate: [smartRedirectGuard],
    children: [] 
  }
];

