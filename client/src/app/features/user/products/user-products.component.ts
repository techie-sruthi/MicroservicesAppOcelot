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
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { Table } from 'primeng/table';
import { ProductService, Product, PagedResult } from '../../../core/services/product.service';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ImageViewerComponent } from '../../../shared/components/image-viewer/image-viewer.component';
import { Subject, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';

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
    ConfirmDialogModule,
    ImageViewerComponent
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './user-products.component.html',
  styleUrl: './user-products.component.css'
})
export class UserProductsComponent implements OnInit, OnDestroy {

  products: Product[] = [];
  loading = false;

  totalRecords = 0;
  pageNumber = 1;
  pageSize = 5;
  first = 0;

  displayDialog = false;
  isEditMode = false;

  selectedFile: File | null = null;
  imagePreview: string | null = null;
  uploading = false;

  searchValue: string = '';
  showFilters: boolean = false;
  minPrice: number | null = null;
  maxPrice: number | null = null;
  startDate: string = '';

  sortField: string | null = null;
  sortOrder: string | null = null;

  expandedDescriptions: Set<string> = new Set();
  today: string = new Date().toISOString().split('T')[0];

  toggleDescription(productId: string) {
    if (this.expandedDescriptions.has(productId)) {
      this.expandedDescriptions.delete(productId);
    } else {
      this.expandedDescriptions.add(productId);
    }
  }

  private productNameCheck$ = new Subject<string>();
  private search$ = new Subject<void>();
  private filter$ = new Subject<void>();
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
    private confirmationService: ConfirmationService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.setupNameValidation();
    this.setupSearchDebounce();
    this.setupFilterDebounce();
  }

  ngOnDestroy(): void {
    this.productNameCheck$.complete();
    this.search$.complete();
    this.filter$.complete();
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
          this.cdr.detectChanges();

          const excludeId = this.isEditMode ? this.productForm.id : undefined;
          return this.productService.checkProductName(name.trim(), excludeId).pipe(
            catchError(() => {
              this.checkingName = false;
              this.cdr.detectChanges();
              return of({ exists: false });
            })
          );
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
          this.cdr.detectChanges();
        },
        error: () => {
          this.checkingName = false;
          this.cdr.detectChanges();
        }
      });
  }

  setupSearchDebounce(): void {
    this.search$.pipe(debounceTime(1000)).subscribe(() => {
      this.pageNumber = 1;
      this.first = 0;
      this.loadProducts();
    });
  }

  setupFilterDebounce(): void {
    this.filter$.pipe(debounceTime(1000)).subscribe(() => {
      this.pageNumber = 1;
      this.first = 0;
      this.loadProducts();
    });
  }

  onProductNameChange(name: string): void {
    this.productNameCheck$.next(name);
  }

  onLazyLoad(event: any): void {
    this.pageNumber = Math.floor(event.first / event.rows) + 1;
    this.pageSize = event.rows;
    this.first = event.first;

    if (event.sortField) {
      this.sortField = event.sortField;
      this.sortOrder = event.sortOrder === 1 ? 'asc' : 'desc';
    }

    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;

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
            detail: 'Failed to load products', styleClass: 'my-custom-toast'
          });
        }
      });
  }

  showCreateDialog(): void {
    this.isEditMode = false;
    this.resetForm();
    this.nameError = '';
    this.checkingName = false;
    this.productNameCheck$.next('');
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
    this.nameError = '';
    this.checkingName = false;
    this.productNameCheck$.next('');
    this.displayDialog = true;
  }

  saveProduct(): void {

    if (this.nameError) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: this.nameError, styleClass: 'my-custom-toast'
      });
      return;
    }

    if (!this.productForm.name || !this.productForm.price || !this.productForm.dateOfManufacture) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'Please fill required fields', styleClass: 'my-custom-toast'
      });
      return;
    }

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
            detail: 'Failed to upload image', styleClass: 'my-custom-toast'
          });
        }
      });
    } else {
      this.saveProductData();
    }
  }

  private saveProductData(): void {
    const productData = {
      ...this.productForm,
      dateOfManufacture: this.productForm.dateOfManufacture + 'T00:00:00.000Z',
      imageUrl: this.productForm.imageUrl
    };

    if (this.isEditMode && this.productForm.id) {
      const updatedId = this.productForm.id;
      this.productService.update(updatedId, productData)
        .subscribe(() => {
          this.products = this.products.map(p =>
            p.id === updatedId ? { ...productData, id: updatedId } as Product : p
          );
          this.cdr.detectChanges();
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Product updated successfully', styleClass: 'my-custom-toast'
          });
          this.displayDialog = false;
        });
    } else {
      this.productService.create(productData as Product)
        .subscribe((response) => {
          const createdProduct: Product = { ...productData, id: response.id } as Product;
          this.products = [createdProduct, ...this.products];
          this.totalRecords++;
          this.cdr.detectChanges();
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Product created successfully', styleClass: 'my-custom-toast'
          });
          this.displayDialog = false;
        });
    }
  }

  deleteProduct(id: string): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to delete this product?',
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-secondary',
      accept: () => {
        this.productService.delete(id).subscribe({
          next: () => {
            this.products = this.products.filter(p => p.id !== id);
            this.totalRecords--;
            this.messageService.add({
              severity: 'success',
              summary: 'Deleted',
              detail: 'Product deleted successfully', styleClass: 'my-custom-toast'
            });
            this.cdr.detectChanges();
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to delete product', styleClass: 'my-custom-toast'
            });
          }
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

    if (!file.type.startsWith('image/')) {
      this.messageService.add({
        severity: 'error',
        summary: 'Invalid File',
        detail: 'Only image files are allowed', styleClass: 'my-custom-toast'
      });
      event.target.value = '';
      return;
    }

        if (file.size > 5 * 1024 * 1024) {
      this.messageService.add({
        severity: 'error',
        summary: 'File Too Large',
        detail: 'Image size must be under 5MB', styleClass: 'my-custom-toast'
      });
      event.target.value = '';
      return;
    }

    this.selectedFile = file;

    const reader = new FileReader();
    reader.onload = (e: any) => {
      this.imagePreview = e.target.result;
    };
    reader.readAsDataURL(file);

    this.messageService.add({
      severity: 'success',
      summary: 'Image Selected',
      detail: `${file.name} (${this.formatFileSize(file.size)})`, styleClass: 'my-custom-toast'
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

  onSearch(): void {
    this.search$.next();
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }

  applyFilters(): void {
    this.filter$.next();
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
      detail: 'All filters have been reset', styleClass: 'my-custom-toast'
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
