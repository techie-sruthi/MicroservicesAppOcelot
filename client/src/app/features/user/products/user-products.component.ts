import { Component, OnInit, ViewChild, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AppLayoutComponent } from '../../../core/layout/app-layout.component';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { ProgressBarModule } from 'primeng/progressbar';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { Table } from 'primeng/table';
import { ProductService, Product, PagedResult } from '../../../core/services/product.service';
import { MessageService } from 'primeng/api';
import { ImageViewerComponent } from '../../../shared/components/image-viewer/image-viewer.component';
import { Subject, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';

interface ProductForm {
  id?: string;
  name: string;
  description?: string;
  price: number;
  dateOfManufacture: string;
  imageUrl?: string;
}

@Component({
  selector: 'app-user-products',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    AppLayoutComponent,
    CardModule,
    TableModule,
    InputTextModule,
    ButtonModule,
    DialogModule,
    InputNumberModule,
    ProgressBarModule,
    ToastModule,
    TooltipModule,
    ImageViewerComponent
  ],
  providers: [MessageService],
  templateUrl: './user-products.component.html',
  styleUrl: './user-products.component.css'
})
export class UserProductsComponent implements OnInit, OnDestroy {

  products: Product[] = [];
  loading = false;

  totalRecords = 0;
  pageNumber = 1;
  pageSize = 5;
  first = 0; // For PrimeNG pagination

  displayDialog = false;
  isEditMode = false;

  selectedFile: File | null = null;
  imagePreview: string | null = null;
  uploading = false;

  // Filter properties
  searchValue: string = '';
  showFilters: boolean = false;
  minPrice: number | null = null;
  maxPrice: number | null = null;
  startDate: string = '';

  // Sorting properties
  sortField: string | null = null;
  sortOrder: string | null = null;

  // Product name validation
  private productNameCheck$ = new Subject<string>();
  nameError: string = '';
  checkingName: boolean = false;

  productForm: ProductForm = {
    name: '',
    description: '',
    price: 0,
    dateOfManufacture: '',
    imageUrl: undefined
  };

  @ViewChild('dt') table!: Table;
  @ViewChild('imageViewer') imageViewer!: ImageViewerComponent;

  constructor(
    private productService: ProductService,
    private messageService: MessageService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.setupNameValidation();
  }

  ngOnDestroy(): void {
    this.productNameCheck$.complete();
  }

  setupNameValidation(): void {
    this.productNameCheck$
      .pipe(
        debounceTime(500),
        distinctUntilChanged(),
        switchMap(name => {
          if (!name || name.trim().length < 2) {
            return of({ exists: false });
          }
          // Set checking state and trigger change detection
          this.checkingName = true;
          this.cdr.detectChanges();

          const excludeId = this.isEditMode ? this.productForm.id : undefined;
          return this.productService.checkProductName(name.trim(), excludeId);
        })
      )
      .subscribe({
        next: (response) => {
          // Defer state changes to next cycle to avoid ExpressionChangedAfterItHasBeenCheckedError
          setTimeout(() => {
            this.checkingName = false;
            if (response.exists) {
              this.nameError = 'A product with this name already exists';
            } else {
              this.nameError = '';
            }
            this.cdr.detectChanges();
          }, 0);
        },
        error: (error) => {
          // Defer error state change too
          setTimeout(() => {
            this.checkingName = false;
            this.cdr.detectChanges();
          }, 0);
        }
      });
  }

  onProductNameChange(name: string): void {
    this.productNameCheck$.next(name);
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

    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;

    // Prepare filter parameters
    const searchTerm = this.searchValue || undefined;
    const minPrice = this.minPrice || undefined;
    const maxPrice = this.maxPrice || undefined;
    const startDate = this.startDate || undefined;
    const sortField = this.sortField || undefined;
    const sortOrder = this.sortOrder || undefined;

    this.productService.getMyProducts(
      this.pageNumber, 
      this.pageSize,
      searchTerm,
      minPrice,
      maxPrice,
      startDate,
      sortField,
      sortOrder
    ).subscribe({
        next: (data: PagedResult<Product>) => {
          this.products = data.items;
          this.totalRecords = data.totalCount;
          this.loading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.loading = false;
          this.cdr.detectChanges();
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load products'
          });
        }
      });
  }

  showCreateDialog(): void {
    this.isEditMode = false;
    this.resetForm();
    this.nameError = '';
    this.checkingName = false;
    this.displayDialog = true;
  }

  editProduct(product: Product): void {
    this.isEditMode = true;
    this.productForm = {
      id: product.id,
      name: product.name,
      description: product.description,
      price: product.price,
      dateOfManufacture: product.dateOfManufacture?.split('T')[0] || '',
      imageUrl: product.imageUrl
    };
    this.imagePreview = product.imageUrl || null;
    this.displayDialog = true;
  }

  saveProduct(): void {

    // Check for validation errors
    if (this.nameError) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: this.nameError
      });
      return;
    }

    if (!this.productForm.name || !this.productForm.price || !this.productForm.dateOfManufacture) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'Please fill required fields'
      });
      return;
    }

    // Upload image first if file is selected
    if (this.selectedFile) {
      this.uploading = true;
      this.productService.uploadImage(this.selectedFile).subscribe({
        next: (response) => {
          this.uploading = false;
          this.productForm.imageUrl = response.imageUrl;
          this.saveProductData();
        },
        error: (error) => {
          this.uploading = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Upload Failed',
            detail: 'Failed to upload image'
          });
        }
      });
    } else {
      this.saveProductData();
    }
  }

  private saveProductData(): void {
    // FIX: Don't use Date constructor to avoid timezone conversion
    const productData = {
      ...this.productForm,
      dateOfManufacture: this.productForm.dateOfManufacture + 'T00:00:00.000Z', // Add time at midnight UTC
      imageUrl: this.productForm.imageUrl
    };

    if (this.isEditMode && this.productForm.id) {
      this.productService.update(this.productForm.id, productData)
        .subscribe(() => this.afterSave('Product updated successfully'));
    } else {
      this.productService.create(productData as Product)
        .subscribe(() => this.afterSave('Product created successfully'));
    }
  }

  private afterSave(message: string): void {
    this.messageService.add({
      severity: 'success',
      summary: 'Success',
      detail: message
    });
    this.displayDialog = false;
    this.loadProducts();
  }

  deleteProduct(id: string): void {
    if (!confirm('Are you sure you want to delete this product?')) return;

    this.productService.delete(id).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Deleted',
          detail: 'Product deleted successfully'
        });
        this.loadProducts();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to delete product'
        });
      }
    });
  }

  resetForm(): void {
    this.productForm = {
      name: '',
      description: '',
      price: 0,
      dateOfManufacture: '',
      imageUrl: undefined
    };
    this.selectedFile = null;
    this.imagePreview = null;
  }

  hideDialog(): void {
    this.displayDialog = false;
    this.nameError = '';
    this.checkingName = false;
    this.resetForm();
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith('image/')) {
      this.messageService.add({
        severity: 'error',
        summary: 'Invalid File',
        detail: 'Only image files are allowed'
      });
      event.target.value = ''; // Clear the input
      return;
    }

    // Validate file size (5MB)
    if (file.size > 5 * 1024 * 1024) {
      this.messageService.add({
        severity: 'error',
        summary: 'File Too Large',
        detail: 'Image size must be under 5MB'
      });
      event.target.value = ''; // Clear the input
      return;
    }

    this.selectedFile = file;

    // Show preview
    const reader = new FileReader();
    reader.onload = (e: any) => {
      this.imagePreview = e.target.result;
    };
    reader.readAsDataURL(file);

    // Show success message
    this.messageService.add({
      severity: 'success',
      summary: 'Image Selected',
      detail: `${file.name} (${this.formatFileSize(file.size)})`
    });
  }

  removeImage(): void {
    this.selectedFile = null;
    this.imagePreview = null;
    this.productForm.imageUrl = undefined;
  }

  viewImage(imageUrl: string): void {
    this.imageViewer.open(imageUrl);
  }

  // Filter methods
  onSearch(): void {
    // Reset to first page when searching
    this.pageNumber = 1;
    this.first = 0;
    this.loadProducts();
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }

  applyFilters(): void {
    // Reset to first page when applying filters
    this.pageNumber = 1;
    this.first = 0;
    this.loadProducts();
  }

  clearFilters(): void {
    this.searchValue = '';
    this.minPrice = null;
    this.maxPrice = null;
    this.startDate = '';
    this.pageNumber = 1;
    this.first = 0;
    this.loadProducts();

    this.messageService.add({
      severity: 'info',
      summary: 'Filters Cleared',
      detail: 'All filters have been reset'
    });
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }
}
