import { Component, NgZone, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { RouterModule } from '@angular/router';

import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { interval, Subscription } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';
import { ChangeDetectorRef } from '@angular/core';


@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    InputText,
    Password,
    Card,
    DialogModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnDestroy {

  loginForm;
  // OTP FEATURE ENABLED
  showOtpDialog = false;
  otpCode = '';
  userEmail = '';
  otpRemaining = 0; // seconds remaining for OTP expiry
  private otpSubscription?: Subscription;

  loading = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private messageService: MessageService,
    private ngZone: NgZone,
    private cd: ChangeDetectorRef 
) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onLogin() {

    if (this.loginForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation Error',
        detail: 'Please fill in all required fields'
      });
      return;
    }

    this.loading = true;
    this.authService.login(this.loginForm.value).subscribe({
      next: (response: any) => {
        this.loading = false;

        // Check if OTP is required
        if (response.otpRequired) {
          this.userEmail = response.email;
          this.showOtpDialog = true;
          this.messageService.add({
            severity: 'info',
            summary: 'OTP Sent',
            detail: response.message || 'Please check your email for OTP'
          });

       
          const expiry = response.expirySeconds ?? 300;
          //const expiry = 10;

          this.startOtpTimer(expiry);
          return;
        }

        // DIRECT LOGIN (OTP BYPASSED)
        // Save tokens first (only when OTP not required)
        if (response.accessToken && response.refreshToken) {
          this.authService.setTokens(response.accessToken, response.refreshToken);

          // Then redirect based on role
          this.handleLoginSuccess(response);
        }

      },
      error: (err: any) => {
        this.loading = false;
        let errorMessage = 'Invalid credentials';
        let errorSummary = 'Login Failed';

        if (err.error?.error) {
          errorMessage = err.error.error;

          if (errorMessage.toLowerCase().includes('user not found')) {
            errorSummary = 'User Not Found';
          } else if (errorMessage.toLowerCase().includes('invalid password')) {
            errorSummary = 'Invalid Password';
          }
        }

        this.messageService.add({
          severity: 'error',
          summary: errorSummary,
          detail: errorMessage,
          life: 5000
        });
      }
    });
  }

  // OTP FEATURE ENABLED
  verifyOtp() {
    if (!this.otpCode || this.otpCode.length !== 6) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Invalid OTP',
        detail: 'Please enter a 6-digit OTP'
      });
      return;
    }

    this.loading = true;
    this.authService.verifyOtp(this.userEmail, this.otpCode).subscribe({
      next: (response) => {
        this.loading = false;
        this.stopOtpTimer();
        this.showOtpDialog = false;
        this.handleLoginSuccess(response);
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Login successful!'
        });
      },
      error: (err) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Verification Failed',
          detail: err.error?.error || 'Invalid or expired OTP'
        });
      }
    });
  }


  handleLoginSuccess(response: any) {
    const role = this.authService.getUserRole();

    if (!role) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Could not determine user role. Please try logging in again.'
      });
      return;
    }

    switch (role.toLowerCase()) {
      case 'admin':
        this.router.navigate(['/admin/products']);
        break;
      case 'user':
        this.router.navigate(['/user/products']);
        break;
      default:
        this.router.navigate(['/user/products']);
    }
  }


  closeOtpDialog() {
    this.stopOtpTimer();
    this.showOtpDialog = false;
    this.otpCode = '';
  }


  goToRegister() {
    this.router.navigate(['/register']);
  }

  // OTP timer helpers
  private startOtpTimer(seconds: number) {

    this.stopOtpTimer();

    this.otpRemaining = seconds > 0 ? seconds : 300;

    this.otpSubscription = interval(1000).subscribe(() => {

      if (this.otpRemaining > 0) {
        this.otpRemaining--;

        // 👇 Force UI update
        this.cd.detectChanges();
      }

      if (this.otpRemaining === 0) {
        this.stopOtpTimer();

        this.messageService.add({
          severity: 'warn',
          summary: 'OTP Expired',
          detail: 'OTP expired. Please login again.'
        });

        this.showOtpDialog = false;
        this.otpCode = '';
      }
    });
  }

  private stopOtpTimer() {
    if (this.otpSubscription) {
      this.otpSubscription.unsubscribe();
      this.otpSubscription = undefined;
    }
  }


  ngOnDestroy(): void {
    this.stopOtpTimer();
  }

  // Format mm:ss
  formatTime(seconds: number): string {
    if (seconds <= 0) return '00:00';

    const m = Math.floor(seconds / 60);
    const s = seconds % 60;

    return `${m.toString().padStart(2, '0')}:${s
      .toString()
      .padStart(2, '0')}`;
  }

}
