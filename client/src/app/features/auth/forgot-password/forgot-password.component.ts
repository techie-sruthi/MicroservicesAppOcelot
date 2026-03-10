import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    CardModule,
    InputTextModule,
    ButtonModule,
    ToastModule,
  ],
  providers: [MessageService],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css',
})
export class ForgotPasswordComponent {
  email: string = '';
  loading: boolean = false;
  emailSent: boolean = false;

  constructor(
    private authService: AuthService,
    private messageService: MessageService,
    private router: Router,
  ) {}

  onSubmit(): void {
    if (!this.email) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Required',
        detail: 'Please enter your email address', styleClass: 'my-custom-toast'
      });
      return;
    }

    this.loading = true;
    this.authService.forgotPassword(this.email).subscribe({
      next: () => {
        this.loading = false;
        this.emailSent = true;
        this.messageService.add({
          severity: 'success',
          summary: 'Email Sent',
          detail: 'If your email exists, you will receive a password reset link.', styleClass: 'my-custom-toast'
        });
      },
      error: () => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Something went wrong. Please try again.', styleClass: 'my-custom-toast'
        });
      },
    });
  }
}
