import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Product {
  id?: string;  // MongoDB uses string IDs (ObjectId)
  name: string;
  description?: string;
  price: number;
  dateOfManufacture: string;
  createdByUserId?: number;
  imageUrl?: string; 
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {

  private apiUrl = `${environment.apiUrl}/products`;  // Gateway routes /products to ProductService

  constructor(private http: HttpClient) { }

  /**
   * Get products based on user role (with pagination):
   * - Admin: Returns all products
   * - User: Returns only user's products
   * Backend handles filtering automatically based on JWT
   */
  getProducts(pageNumber: number = 1, pageSize: number = 10): Observable<PagedResult<Product>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<PagedResult<Product>>(this.apiUrl, { params });
  }

  /**
   * Get all products (Admin only) with pagination and filters
   */
  getAllProducts(
    pageNumber: number = 1, 
    pageSize: number = 10,
    searchTerm?: string,
    minPrice?: number,
    maxPrice?: number,
    startDate?: string,
    sortField?: string,
    sortOrder?: string
  ): Observable<PagedResult<Product>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (minPrice !== undefined && minPrice !== null) {
      params = params.set('minPrice', minPrice.toString());
    }
    if (maxPrice !== undefined && maxPrice !== null) {
      params = params.set('maxPrice', maxPrice.toString());
    }
    if (startDate) {
      params = params.set('startDate', startDate);
    }
    if (sortField) {
      params = params.set('sortField', sortField);
    }
    if (sortOrder) {
      params = params.set('sortOrder', sortOrder);
    }

    return this.http.get<PagedResult<Product>>(`${this.apiUrl}/all`, { params });
  }

  /**
   * Get only the logged-in user's products with pagination and filters
   */
  getMyProducts(
    pageNumber: number = 1, 
    pageSize: number = 10,
    searchTerm?: string,
    minPrice?: number,
    maxPrice?: number,
    startDate?: string,
    sortField?: string,
    sortOrder?: string
  ): Observable<PagedResult<Product>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (minPrice !== undefined && minPrice !== null) {
      params = params.set('minPrice', minPrice.toString());
    }
    if (maxPrice !== undefined && maxPrice !== null) {
      params = params.set('maxPrice', maxPrice.toString());
    }
    if (startDate) {
      params = params.set('startDate', startDate);
    }
    if (sortField) {
      params = params.set('sortField', sortField);
    }
    if (sortOrder) {
      params = params.set('sortOrder', sortOrder);
    }

    return this.http.get<PagedResult<Product>>(`${this.apiUrl}/my-products`, { params });
  }


  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  getByUserId(userId: number): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/user/${userId}`);
  }

  create(product: Product): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, product);
  }

  update(id: string, product: Product): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, { ...product, id });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  uploadImage(file: File): Observable<{ imageUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ imageUrl: string }>(`${this.apiUrl}/upload-image`, formData);
  }

  checkProductName(name: string, excludeId?: string): Observable<{ exists: boolean }> {
    let params = new HttpParams().set('name', name);
    if (excludeId) {
      params = params.set('excludeId', excludeId);
    }
    return this.http.get<{ exists: boolean }>(`${this.apiUrl}/check-name`, { params });
  }
}
