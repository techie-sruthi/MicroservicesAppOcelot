import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { RouterModule } from '@angular/router';

import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

import { AuthService } from '../../../core/services/auth.service';
import { Subject, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    InputText,
    Password,
    Card,
    RouterModule,
    ToastModule,
  ],
  providers: [MessageService],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent implements OnInit, OnDestroy {
  registerForm;
  loading = false;

  // Email validation
  private emailCheck$ = new Subject<string>();
  emailError: string = '';
  checkingEmail: boolean = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private messageService: MessageService,
  ) {
    this.registerForm = this.fb.group({
      userName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.setupEmailValidation();
  }

  ngOnDestroy(): void {
    this.emailCheck$.complete();
  }

  setupEmailValidation(): void {
    this.emailCheck$
      .pipe(
        debounceTime(500),
        distinctUntilChanged(),
        switchMap((email) => {
          // Check if email is valid format and not empty
          if (!email || !this.isValidEmail(email)) {
            return of({ exists: false });
          }
          this.checkingEmail = true;
          return this.authService.checkEmail(email.trim().toLowerCase());
        }),
      )
      .subscribe({
        next: (response) => {
          this.checkingEmail = false;
          if (response.exists) {
            this.emailError = 'This email is already registered';
          } else {
            this.emailError = '';
          }
        },
        error: (error) => {
          this.checkingEmail = false;
          console.error('Email check error:', error);
        },
      });
  }

  isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  onEmailChange(email: string): void {
    this.emailCheck$.next(email);
  }

  onRegister() {
    // Check for email validation error
    if (this.emailError) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: this.emailError,
        styleClass: 'my-custom-toast',
      });
      return;
    }

    if (this.registerForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation Error',
        detail: 'Please fill in all required fields',
        styleClass: 'my-custom-toast',
      });
      return;
    }

    const { confirmPassword, ...registerData } = this.registerForm.value;

    if (this.registerForm.value.password !== confirmPassword) {
      this.messageService.add({
        severity: 'error',
        summary: 'Password Mismatch',
        detail: 'Passwords do not match',
        styleClass: 'my-custom-toast',
      });
      return;
    }

    this.loading = true;

    const requestData = {
      ...registerData,
      role: 'User',
    };

    this.authService.register(requestData).subscribe({
      next: (response) => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Registration successful! Please login.',
          styleClass: 'my-custom-toast',
          life: 3000,
        });
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (err) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Registration Failed',
          detail: err.error?.message || 'Failed to register. Please try again.',
          styleClass: 'my-custom-toast',
          life: 5000,
        });
      },
    });
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}
