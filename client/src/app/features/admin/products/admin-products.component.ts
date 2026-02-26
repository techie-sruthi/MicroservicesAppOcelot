import { Component, OnInit, ViewChild, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AppLayoutComponent } from '../../../core/layout/app-layout.component';
import { ProductService, Product, PagedResult } from '../../../core/services/product.service';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { ProgressBarModule } from 'primeng/progressbar';
import { Table } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
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
  selector: 'app-admin-products',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    AppLayoutComponent, 
    CardModule, 
    InputTextModule,
    InputNumberModule, 
    ButtonModule, 
    TableModule, 
    DialogModule,
    ProgressBarModule,
    ToastModule,
    TooltipModule,
    ImageViewerComponent
  ],
  providers: [MessageService],
  templateUrl: './admin-products.component.html',
  styleUrl: './admin-products.component.css'
})
export class AdminProductsComponent implements OnInit, OnDestroy {

  products: Product[] = [];
  searchValue: string = '';
  loading: boolean = false;
  displayDialog: boolean = false;
  isEditMode: boolean = false;

  // Pagination properties
  totalRecords: number = 0;
  pageNumber: number = 1;
  pageSize: number = 5; // Match the first option in rowsPerPageOptions
  first: number = 0; // For PrimeNG pagination

  // Image upload
  selectedFile: File | null = null;
  imagePreview: string | null = null;
  uploading: boolean = false;

  // Filter properties
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
    this.loadProducts();
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
          this.checkingName = true;
          const excludeId = this.isEditMode ? this.productForm.id : undefined;
          return this.productService.checkProductName(name.trim(), excludeId);
        })
      )
      .subscribe({
        next: (response) => {
          this.checkingName = false;
          if (response.exists) {
            this.nameError = 'A product with this name already exists';
          } else {
            this.nameError = '';
          }
        },
        error: (error) => {
          this.checkingName = false;
        }
      });
  }

  onProductNameChange(name: string): void {
    this.productNameCheck$.next(name);
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
    
    this.productService.getAllProducts(
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

        if (data.items.length === 0 && data.totalCount === 0) {
          this.messageService.add({ 
            severity: 'info', 
            summary: 'No Products', 
            detail: 'No products found matching your criteria.' 
          });
        }
      },
      error: (err) => {
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

  showCreateDialog() {
    this.isEditMode = false;
    this.productForm = {
      name: '',
      description: '',
      price: 0,
      dateOfManufacture: '',
      imageUrl: undefined
    };
    this.selectedFile = null;
    this.imagePreview = null;
    this.nameError = '';
    this.checkingName = false;
    this.displayDialog = true;
  }

  editProduct(product: Product) {
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
    // Trigger change detection to ensure form values are displayed immediately
    this.cdr.detectChanges();
  }

  async saveProduct() {
    // Check for validation errors
    if (this.nameError) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: this.nameError
      });
      return;
    }

    // Validate required fields
    if (!this.productForm.name || !this.productForm.price || 
        !this.productForm.dateOfManufacture) {
      this.messageService.add({ 
        severity: 'warn', 
        summary: 'Validation', 
        detail: 'Please fill all required fields' 
      });
      return;
    }

    // Upload image if selected
    let imageUrl = this.productForm.imageUrl;
    if (this.selectedFile) {
      this.uploading = true;
      try {
        const uploadResult = await this.productService.uploadImage(this.selectedFile).toPromise();
        imageUrl = uploadResult?.imageUrl;
        this.messageService.add({
          severity: 'success',
          summary: 'Image Uploaded',
          detail: 'Product image uploaded successfully'
        });
      } catch (error) {
        this.messageService.add({
          severity: 'error',
          summary: 'Upload Failed',
          detail: 'Failed to upload image'
        });
        this.uploading = false;
        return;
      }
      this.uploading = false;
    }

    // Prepare product data - FIX: Don't use Date constructor, use date string directly
    const productData = {
      ...this.productForm,
      dateOfManufacture: this.productForm.dateOfManufacture + 'T00:00:00.000Z', // Add time at midnight UTC
      imageUrl: imageUrl
    };

    if (this.isEditMode && this.productForm.id) {
      // Update existing product
      this.productService.update(this.productForm.id, productData).subscribe({
        next: () => {
          this.messageService.add({ 
            severity: 'success', 
            summary: 'Success', 
            detail: 'Product updated successfully' 
          });
          this.hideDialog();
          this.loadProducts();
        },
        error: (err) => {
          this.messageService.add({ 
            severity: 'error', 
            summary: 'Error', 
            detail: 'Failed to update product' 
          });
        }
      });
    } else {
      // Create new product
      this.productService.create(productData as Product).subscribe({
        next: () => {
          this.messageService.add({ 
            severity: 'success', 
            summary: 'Success', 
            detail: 'Product created successfully' 
          });
          this.hideDialog();
          this.loadProducts();
        },
        error: (err) => {
          this.messageService.add({ 
            severity: 'error', 
            summary: 'Error', 
            detail: 'Failed to create product' 
          });
        }
      });
    }
  }

  deleteProduct(id: string) {
    if (confirm('Are you sure you want to delete this product?')) {
      this.productService.delete(id).subscribe({
        next: () => {
          this.messageService.add({ 
            severity: 'success', 
            summary: 'Success', 
            detail: 'Product deleted successfully' 
          });
          this.loadProducts();
        },
        error: (err) => {
          this.messageService.add({ 
            severity: 'error', 
            summary: 'Error', 
            detail: 'Failed to delete product' 
          });
        }
      });
    }
  }

  hideDialog() {
    this.displayDialog = false;
    this.productForm = {
      name: '',
      description: '',
      price: 0,
      dateOfManufacture: '',
      imageUrl: undefined
    };
    this.selectedFile = null;
    this.imagePreview = null;
    this.nameError = '';
    this.checkingName = false;
  }

  onSearch() {
    // Reset to first page when searching
    this.pageNumber = 1;
    this.first = 0;
    // loadProducts will be called by onLazyLoad
    this.cdr.detectChanges();
    this.loadProducts();
  }

  applyFilters() {
    // Reset to first page when applying filters
    this.pageNumber = 1;
    this.first = 0;
    this.cdr.detectChanges();
    this.loadProducts();
  }

  toggleFilters() {
    this.showFilters = !this.showFilters;
  }

  clearFilters() {
    this.searchValue = '';
    this.minPrice = null;
    this.maxPrice = null;
    this.startDate = '';
    this.pageNumber = 1;
    this.first = 0;
    this.cdr.detectChanges();
    this.loadProducts();

    this.messageService.add({
      severity: 'info',
      summary: 'Filters Cleared',
      detail: 'All filters have been reset'
    });
  }

  // Image handling methods
  onFileSelected(event: any) {
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

    // Generate preview
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

  removeImage() {
    this.selectedFile = null;
    this.imagePreview = null;
    this.productForm.imageUrl = undefined;
  }

  viewImage(imageUrl: string) {
    this.imageViewer.open(imageUrl);
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }
}
