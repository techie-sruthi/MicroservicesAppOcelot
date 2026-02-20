import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { Password } from 'primeng/password';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    CardModule,
    InputTextModule,
    ButtonModule,
    ToastModule,
    Password
  ],
  providers: [MessageService],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent implements OnInit {
  token: string = '';
  newPassword: string = '';
  confirmPassword: string = '';
  loading: boolean = false;
  resetSuccess: boolean = false;
  tokenInvalid: boolean = false;

  constructor(
    private authService: AuthService,
    private messageService: MessageService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') || '';
    if (!this.token) {
      this.tokenInvalid = true;
    }
  }

  onSubmit(): void {
    if (!this.newPassword || !this.confirmPassword) {
      this.messageService.add({ severity: 'warn', summary: 'Required', detail: 'Please fill in both password fields' });
      return;
    }

    if (this.newPassword.length < 6) {
      this.messageService.add({ severity: 'warn', summary: 'Too Short', detail: 'Password must be at least 6 characters' });
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.messageService.add({ severity: 'warn', summary: 'Mismatch', detail: 'Passwords do not match' });
      return;
    }

    this.loading = true;
    this.authService.resetPassword(this.token, this.newPassword).subscribe({
      next: () => {
        this.loading = false;
        this.resetSuccess = true;
        this.messageService.add({
          severity: 'success',
          summary: 'Password Reset',
          detail: 'Your password has been reset successfully!'
        });
      },
      error: (err) => {
        this.loading = false;
        const message = err.error?.message || 'Invalid or expired reset token. Please request a new one.';
        this.messageService.add({
          severity: 'error',
          summary: 'Reset Failed',
          detail: message
        });
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
