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
    ToastModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.css'
})
export class AdminUsersComponent implements OnInit {
  users: User[] = [];
  searchValue: string = '';
  selectedUser: User = { id: 0, userName: '', email: '', role: 'User' };
  isEditMode = false;
  loading = false;
  saving = false;
  roleOptions = [
    { label: 'User', value: 'User' },
    { label: 'Admin', value: 'Admin' }
  ];

  totalRecords: number = 0;
  pageNumber: number = 1;
  pageSize: number = 5;
  first: number = 0;

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

    this.userService.getAllUsers(
      this.pageNumber,
      this.pageSize,
      this.searchValue || undefined,
      undefined,
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

    if (event.sortField) {
      this.sortField = event.sortField;
      this.sortOrder = event.sortOrder === 1 ? 'asc' : 'desc';
    }

    this.loadUsers();
  }

  saveUser() {
    if (!this.selectedUser.userName || !this.selectedUser.email || !this.selectedUser.role) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation Error',
        detail: 'Please fill in all required fields'
      });
      return;
    }

    const currentUserId = this.authService.getUserId();
    const isChangingOwnRole = this.isEditMode && 
                              this.selectedUser.id === currentUserId && 
                              this.selectedUser.role !== this.authService.getUserRole();

    if (isChangingOwnRole) {
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
          this.performSaveUser(true); 
        },
        reject: () => {
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
      this.userService.updateUser(this.selectedUser.id, this.selectedUser)
        .subscribe({
          next: () => {
            this.saving = false;

            if (autoLogoutAfter) {
              this.messageService.add({
                severity: 'success',
                summary: 'Role Changed',
                detail: 'Your role has been updated. Logging out in 3 seconds...',
                life: 3000
              });

              setTimeout(() => {
                this.authService.logout();
                this.router.navigate(['/login'], {
                  queryParams: { message: 'Your role has been changed. Please login again.' }
                });
              }, 3000);
            } else {
              const updatedUser = { ...this.selectedUser };
              this.users = this.users.map(u =>
                u.id === updatedUser.id ? updatedUser : u
              );
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'User updated successfully'
              });
              this.resetForm();
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
    
      const createUserData = {
        ...this.selectedUser
      };

      this.userService.createUser(createUserData)
        .subscribe({
          next: (response: any) => {
            this.saving = false;
            const createdUser: User = {
              ...this.selectedUser,
              id: typeof response === 'number' ? response : response?.id ?? 0
            };
            this.totalRecords++;
            this.users = [createdUser, ...this.users];
            this.cdr.detectChanges();
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'User created successfully. Password has been sent to their email.'
            });
            this.resetForm();
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
            this.users = this.users.filter(u => u.id !== id);
            this.totalRecords--;
            this.cdr.detectChanges();
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'User deleted successfully'
            });
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
    this.isEditMode = false;
  }

  onSearch() {
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
