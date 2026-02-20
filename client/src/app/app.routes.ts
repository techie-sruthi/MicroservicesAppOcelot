import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { ForgotPasswordComponent } from './features/auth/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './features/auth/reset-password/reset-password.component';
import { UserDashboardComponent } from './features/user/user-dashboard/user-dashboard.component';
import { AdminDashboardComponent } from './features/admin/admin-dashboard/admin-dashboard.component';

import { AdminUsersComponent } from './features/admin/users/admin-users.component';
import { AdminProductsComponent } from './features/admin/products/admin-products.component';
import { UserProductsComponent } from './features/user/products/user-products.component';

import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { smartRedirectGuard } from './core/guards/smart-redirect.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },

  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'reset-password', component: ResetPasswordComponent },

  //{
  //  path: 'user-dashboard',
  //  canActivate: [authGuard, roleGuard],
  //  data: { role: 'User' },
  //  component: UserDashboardComponent
  //},
  //{
  //  path: 'admin-dashboard',
  //  canActivate: [authGuard, roleGuard],
  //  data: { role: 'Admin' },
  //  component: AdminDashboardComponent
  //},

  // ADMIN - protected
  {
    path: 'admin/products',
    canActivate: [authGuard, roleGuard],
    data: { role: 'Admin' },
    component: AdminProductsComponent
  },
  {
    path: 'admin/users',
    canActivate: [authGuard, roleGuard],
    data: { role: 'Admin' },
    component: AdminUsersComponent
  },

  // USER - protected
  {
    path: 'user/products',
    canActivate: [authGuard, roleGuard],
    data: { role: 'User' },
    component: UserProductsComponent
  },

  // Unknown routes - smart redirect based on authentication
  { 
    path: '**', 
    canActivate: [smartRedirectGuard],
    children: [] 
  }
];

