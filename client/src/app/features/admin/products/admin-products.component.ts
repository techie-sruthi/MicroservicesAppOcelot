import { Component, OnInit, ViewChild, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AppLayoutComponent } from '../../../core/layout/app-layout.component';
import {
  ProductService,
  Product,
  PagedResult,
  MergedProduct,
} from '../../../core/services/product.service';
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
import { ConfirmDialogModule } from 'primeng/confirmdialog';
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
    ConfirmDialogModule,
    ImageViewerComponent,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './admin-products.component.html',
  styleUrl: './admin-products.component.css',
})
export class AdminProductsComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  mergedProducts: Product[] = [];

  searchValue: string = '';
  loading: boolean = false;
  displayDialog: boolean = false;
  isEditMode: boolean = false;

  totalRecords: number = 0;
  pageNumber: number = 1;
  pageSize: number = 5;
  first: number = 0;

  selectedFile: File | null = null;
  imagePreview: string | null = null;
  uploading: boolean = false;
  imageLoading: boolean = false;

  showFilters: boolean = false;
  minPrice: number | null = null;
  maxPrice: number | null = null;
  startDate: string = '';

  sortField: string | null = null;
  sortOrder: string | null = null;

  expandedDescriptions: Set<number> = new Set();
  today: string = new Date().toISOString().split('T')[0];

  toggleDescription(productId: number) {
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
    imageUrl: undefined,
  };

  @ViewChild('dt') table!: Table;
  @ViewChild('imageViewer') imageViewer!: ImageViewerComponent;

  constructor(
    private productService: ProductService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private cdr: ChangeDetectorRef,
  ) {}

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
        switchMap((name) => {
          if (!name || name.trim().length < 2) {
            return of({ exists: false });
          }
          this.checkingName = true;
          const excludeId = this.isEditMode ? this.productForm.id : undefined;
          return this.productService.checkProductName(name.trim(), excludeId).pipe(
            catchError(() => {
              this.checkingName = false;
              this.cdr.detectChanges();
              return of({ exists: false });
            }),
          );
        }),
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
        },
      });
  }

  setupSearchDebounce(): void {
    this.search$.pipe(debounceTime(1000)).subscribe(() => {
      this.pageNumber = 1;
      this.first = 0;
      this.cdr.detectChanges();
      this.loadProducts();
    });
  }

  setupFilterDebounce(): void {
    this.filter$.pipe(debounceTime(1000)).subscribe(() => {
      this.pageNumber = 1;
      this.first = 0;
      this.cdr.detectChanges();
      this.loadProducts();
    });
  }

  onProductNameChange(name: string): void {
    this.productNameCheck$.next(name);
  }

  loadRefreshProducts(): void {
    this.searchValue = '';
    this.minPrice = null;
    this.maxPrice = null;
    this.startDate = '';
    this.pageNumber = 1;
    this.first = 0;
    this.sortField = null;
    this.sortOrder = null;
    this.showFilters = false;
    this.cdr.detectChanges();
    if (this.table) {
      this.table.sortField = '';
      this.table.sortOrder = 1;
      this.table.multiSortMeta = null;
      this.table.tableService.onSort(null);
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

    this.productService
      .getAllProducts(
        this.pageNumber,
        this.pageSize,
        searchTerm,
        minPrice,
        maxPrice,
        startDate,
        sortField,
        sortOrder,
      )
      .subscribe({
        next: (data: PagedResult<Product>) => {
          this.products = data.items;
          this.totalRecords = data.totalCount;
          this.loading = false;
          this.cdr.detectChanges();

          if (data.items.length === 0 && data.totalCount === 0) {
            this.messageService.add({
              severity: 'info',
              summary: 'No Products',
              detail: 'No products found matching your criteria.',
              styleClass: 'my-custom-toast',
            });
          }
        },
        error: (err) => {
          this.loading = false;
          this.cdr.detectChanges();
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load products',
            styleClass: 'my-custom-toast',
          });
        },
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

    this.loadProducts();
  }

  showCreateDialog() {
    this.isEditMode = false;
    this.productForm = {
      name: '',
      description: '',
      price: 0,
      dateOfManufacture: '',
      imageUrl: undefined,
    };
    this.selectedFile = null;
    this.imagePreview = null;
    this.nameError = '';
    this.checkingName = false;
    this.productNameCheck$.next('');
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
      imageUrl: product.imageUrl,
    };
    this.imagePreview = product.imageUrl || null;
    this.nameError = '';
    this.checkingName = false;
    this.productNameCheck$.next('');
    this.displayDialog = true;
    this.cdr.detectChanges();
  }

  async saveProduct() {
    if (this.nameError) {
      this.messageService.add({
        severity: 'error',
        summary: 'Validation Error',
        detail: this.nameError,
        styleClass: 'my-custom-toast',
      });
      return;
    }

    if (!this.productForm.name || !this.productForm.price || !this.productForm.dateOfManufacture) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Validation',
        detail: 'Please fill all required fields',
        styleClass: 'my-custom-toast',
      });
      return;
    }

    let imageUrl = this.productForm.imageUrl;
    if (this.selectedFile) {
      this.uploading = true;
      try {
        const uploadResult = await this.productService.uploadImage(this.selectedFile).toPromise();
        imageUrl = uploadResult?.imageUrl;
        this.messageService.add({
          severity: 'success',
          summary: 'Image Uploaded',
          detail: 'Product image uploaded successfully',
          styleClass: 'my-custom-toast',
        });
      } catch (error) {
        this.messageService.add({
          severity: 'error',
          summary: 'Upload Failed',
          detail: 'Failed to upload image',
          styleClass: 'my-custom-toast',
        });
        this.uploading = false;
        return;
      }
      this.uploading = false;
    }

    const productData = {
      ...this.productForm,
      dateOfManufacture: this.productForm.dateOfManufacture + 'T00:00:00.000Z',
      imageUrl: imageUrl,
    };

    if (this.isEditMode && this.productForm.id) {
      const updatedId = this.productForm.id;
      this.productService.update(updatedId, productData).subscribe({
        next: () => {
          this.products = this.products.map((p) =>
            p.id === updatedId ? ({ ...productData, id: updatedId } as Product) : p,
          );
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Product updated successfully',
            styleClass: 'my-custom-toast',
          });
          this.hideDialog();
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to update product',
            styleClass: 'my-custom-toast',
          });
        },
      });
    } else {
      this.productService.create(productData as Product).subscribe({
        next: (response) => {
          const createdProduct: Product = { ...productData, id: response.id } as Product;
          this.products = [createdProduct, ...this.products];
          this.totalRecords++;
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Product created successfully',
            styleClass: 'my-custom-toast',
          });
          this.hideDialog();
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to create product',
            styleClass: 'my-custom-toast',
          });
        },
      });
    }
  }

  deleteProduct(id: string) {
    this.confirmationService.confirm({
      message: 'Are you sure you want to delete this product?',
      header: 'Confirm Delete',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-secondary',
      accept: () => {
        this.productService.delete(id).subscribe({
          next: () => {
            this.products = this.products.filter((p) => p.id !== id);
            this.totalRecords--;
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Product deleted successfully',
              styleClass: 'my-custom-toast',
            });
            this.cdr.detectChanges();
          },
          error: (err) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to delete product',
              styleClass: 'my-custom-toast',
            });
          },
        });
      },
    });
  }

  hideDialog() {
    this.displayDialog = false;
    this.productForm = {
      name: '',
      description: '',
      price: 0,
      dateOfManufacture: '',
      imageUrl: undefined,
    };
    this.selectedFile = null;
    this.imagePreview = null;
    this.nameError = '';
    this.checkingName = false;
  }

  onSearch() {
    this.search$.next();
  }

  applyFilters() {
    this.filter$.next();
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
      detail: 'All filters have been reset',
      styleClass: 'my-custom-toast',
      life: 3000,
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.messageService.add({
        severity: 'error',
        summary: 'Invalid File',
        detail: 'Only image files are allowed',
        styleClass: 'my-custom-toast',
      });
      event.target.value = '';
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      this.messageService.add({
        severity: 'error',
        summary: 'File Too Large',
        detail: 'Image size must be under 5MB',
        styleClass: 'my-custom-toast',
      });
      event.target.value = '';
      return;
    }

    this.selectedFile = file;
    this.imageLoading = true;

    const reader = new FileReader();
    reader.onload = (e: any) => {
      this.imagePreview = e.target.result;
      this.imageLoading = false;
      this.cdr.detectChanges();
      this.messageService.add({
        severity: 'success',
        summary: 'Image Selected',
        detail: `${file.name} (${this.formatFileSize(file.size)})`,
        styleClass: 'my-custom-toast',
      });
    };
    reader.onerror = () => {
      this.imageLoading = false;
      this.cdr.detectChanges();
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to read the selected image',
        styleClass: 'my-custom-toast',
      });
    };
    reader.readAsDataURL(file);
  }

  removeImage() {
    this.selectedFile = null;
    this.imagePreview = null;
    this.imageLoading = false;
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
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }
}
