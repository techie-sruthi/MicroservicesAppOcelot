import { Component, OnInit, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AppLayoutComponent } from '../../../core/layout/app-layout.component';
import { UserService, User, PagedResult } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { distinctUntilChanged, Observable, tap } from 'rxjs';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { SelectModule } from 'primeng/select';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { Password } from 'primeng/password';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    AppLayoutComponent,
    CardModule,
    InputTextModule,
    ButtonModule,
    TableModule,
    SelectModule,
    ConfirmDialogModule,
    ToastModule,
    Password
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.css'
})
export class AdminUsersComponent implements OnInit {
  users: User[] = [];
  searchValue: string = '';
  selectedUser: User = { id: 0, userName: '', email: '', role: 'User' };
  userPassword: string = ''; // Separate field for password
  isEditMode = false;
  loading = false;
  saving = false;
  roleOptions = [
    { label: 'User', value: 'User' },
    { label: 'Admin', value: 'Admin' }
  ];

  totalRecords: number = 0;
  pageNumber: number = 1;
  pageSize: number = 5; // Match the first option in rowsPerPageOptions
  first: number = 0; // For PrimeNG pagination

  // Sorting properties
  sortField: string | null = null;
  sortOrder: string | null = null;

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  isEditingOwnAccount(): boolean {
    const currentUserId = this.authService.getUserId();
    return this.isEditMode && this.selectedUser.id === currentUserId;
  }

  loadUsers() {
    this.loading = true;
console.log('Loading users with parameters:', {
  pageNumber: this.pageNumber,
  pageSize: this.pageSize,
  searchValue: this.searchValue,
  sortField: this.sortField,
  sortOrder: this.sortOrder
});

    this.userService.getAllUsers(
      this.pageNumber,
      this.pageSize,
      this.searchValue || undefined,
      undefined, // roleFilter (not used here)
      this.sortField || undefined,
      this.sortOrder || undefined
    ).subscribe({
      next: (data: PagedResult<User>) => {

        this.users = data.items;
        this.totalRecords = data.totalCount;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.loading = false;
        this.cdr.detectChanges();
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load users'
        });
      }
    });
  }

  onLazyLoad(event: any): void {


    this.pageNumber = Math.floor(event.first / event.rows) + 1;
    this.pageSize = event.rows;
    this.first = event.first;

    // Handle sorting from PrimeNG
    if (event.sortField) {
      this.sortField = event.sortField;
      this.sortOrder = event.sortOrder === 1 ? 'asc' : 'desc';
    }

    this.loadUsers();
  }

  saveUser() {
    // Validate required fields
    if (!this.selectedUser.userName || !this.selectedUser.email || !this.selectedUser.role) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation Error',
        detail: 'Please fill in all required fields'
      });
      return;
    }

    // Password is required only for creating new users
    if (!this.isEditMode && !this.userPassword) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation Error',
        detail: 'Password is required for new users'
      });
      return;
    }

    // Validate password strength for new users
    if (!this.isEditMode && this.userPassword.length < 6) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation Error',
        detail: 'Password must be at least 6 characters long'
      });
      return;
    }

    // Check if admin is changing their own role
    const currentUserId = this.authService.getUserId();
    const isChangingOwnRole = this.isEditMode && 
                              this.selectedUser.id === currentUserId && 
                              this.selectedUser.role !== this.authService.getUserRole();

    if (isChangingOwnRole) {
      // Warn admin about role change
      this.confirmationService.confirm({
        message: 'You are changing your own role! After saving, you will need to logout and login again for the changes to take effect. Do you want to continue?',
        header: 'Role Change Warning',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Yes, Continue',
        rejectLabel: 'Cancel',
        acceptButtonStyleClass: 'p-button-warning',
        rejectButtonStyleClass: 'p-button-secondary',
        closeOnEscape: true,
        accept: () => {
          this.performSaveUser(true); // true = auto logout after save
        },
        reject: () => {
          // Dialog will close automatically
          this.messageService.add({
            severity: 'info',
            summary: 'Cancelled',
            detail: 'Role change cancelled'
          });
        }
      });
      return;
    }

    this.performSaveUser(false);
  }

  private performSaveUser(autoLogoutAfter: boolean) {
    this.saving = true;

    if (this.isEditMode && this.selectedUser.id) {
      // Update existing user (without password)
      this.userService.updateUser(this.selectedUser.id, this.selectedUser)
        .subscribe({
          next: () => {
            this.saving = false;

            if (autoLogoutAfter) {
              // Admin changed their own role
              this.messageService.add({
                severity: 'success',
                summary: 'Role Changed',
                detail: 'Your role has been updated. Logging out in 3 seconds...',
                life: 3000
              });

              // Auto logout after 3 seconds
              setTimeout(() => {
                this.authService.logout();
                this.router.navigate(['/login'], {
                  queryParams: { message: 'Your role has been changed. Please login again.' }
                });
              }, 3000);
            } else {
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'User updated successfully'
              });
              this.resetForm();
              setTimeout(() => {
                this.loadUsers();
              }, 100);
            }
          },
          error: (err) => {
            this.saving = false;
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: err.error?.message || 'Failed to update user'
            });
          }
        });
    } else {
      // Create new user (with password)
      const createUserData = {
        ...this.selectedUser,
        password: this.userPassword
      };

      this.userService.createUser(createUserData)
        .subscribe({
          next: () => {
            this.saving = false;
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'User created successfully'
            });
            this.resetForm();
            setTimeout(() => {
              this.loadUsers();
            }, 100);
          },
          error: (err) => {
            this.saving = false;
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: err.error?.message || 'Failed to create user'
            });
          }
        });
    }
  }

  editUser(user: User) {
    this.selectedUser = { ...user };
    this.userPassword = ''; // Clear password when editing
    this.isEditMode = true;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  deleteUser(id: number) {
    this.confirmationService.confirm({
      message: 'Are you sure you want to delete this user? This action cannot be undone.',
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-secondary',
      accept: () => {
        this.userService.deleteUser(id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'User deleted successfully'
            });
            this.loadUsers();
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: err.error?.message || 'Failed to delete user'
            });
          }
        });
      }
    });
  }

  resetForm() {
    this.selectedUser = { id: 0, userName: '', email: '', role: 'User' };
    this.userPassword = '';
    this.isEditMode = false;
  }

  onSearch() {
    // Server-side search: reset to first page and reload with searchValue
    this.pageNumber = 1;
    this.first = 0;
    this.loadUsers();
  }

  clearSearch() {
    this.searchValue = '';
    this.pageNumber = 1;
    this.first = 0;
    this.cdr.detectChanges();
    this.loadUsers();
  }
}
