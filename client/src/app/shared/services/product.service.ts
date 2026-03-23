import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { map } from 'rxjs/operators';
import { ApiResponse } from '../models/api-response.model';

export interface IProduct {
  id?: string;
  name: string;
  description?: string;
  price: number;
  dateOfManufacture: string;
  createdByUserId?: number;
  imageUrl?: string;
}

export interface IMergedProduct {
  id?: string;
  name: string;
  description?: string;
  price: number;
  dateOfManufacture: string;
  createdByUserId?: number;
  createdByUserName?: string;
  imageUrl?: string;
}

export interface IPagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private apiUrl = `${environment.apiUrl}/products`; // Gateway routes /products to ProductService

  constructor(private http: HttpClient) {}

  getProducts(pageNumber: number = 1, pageSize: number = 10): Observable<IPagedResult<IProduct>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<ApiResponse<IPagedResult<IProduct>>>(this.apiUrl, { params }).pipe(
      map(res => res.data!)
    );
  }

  getAllProducts(
    pageNumber: number = 1,
    pageSize: number = 10,
    searchTerm?: string,
    minPrice?: number,
    maxPrice?: number,
    startDate?: string,
    sortField?: string,
    sortOrder?: string,
  ): Observable<IPagedResult<IProduct>> {
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

    return this.http.get<ApiResponse<IPagedResult<IProduct>>>(`${this.apiUrl}/GetAllProducts`, { params }).pipe(
      map(res => res.data!)
    );
  }

  getMyProducts(
    pageNumber: number = 1,
    pageSize: number = 10,
    searchTerm?: string,
    minPrice?: number,
    maxPrice?: number,
    startDate?: string,
    sortField?: string,
    sortOrder?: string,
  ): Observable<IPagedResult<IProduct>> {
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

    return this.http.get<ApiResponse<IPagedResult<IProduct>>>(`${this.apiUrl}/GetMyProducts`, { params }).pipe(
      map(res => res.data!)
    );
  }

  getById(id: string): Observable<IProduct> {
    return this.http.get<ApiResponse<IProduct>>(`${this.apiUrl}/GetById/${id}`).pipe(
      map(res => res.data!)
    );
  }

  getByUserId(userId: number): Observable<IProduct[]> {
    return this.http.get<ApiResponse<IProduct[]>>(`${this.apiUrl}/user/${userId}`).pipe(
      map(res => res.data!)
    );
  }

  create(product: IProduct): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/Create`, product);
  }

  update(id: string, product: IProduct): Observable<ApiResponse<object>> {
    return this.http.put<ApiResponse<object>>(`${this.apiUrl}/Update/${id}`, { ...product, id });
  }

  delete(id: string): Observable<ApiResponse<object>> {
  return this.http.delete<ApiResponse<object>>(`${this.apiUrl}/Delete/${id}`);
}

  uploadImage(file: File): Observable<ApiResponse<string>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/UploadImage`, formData);
  }

  checkProductName(name: string, excludeId?: string): Observable<{ exists: boolean }> {
    let params = new HttpParams().set('name', name);
    if (excludeId) {
      params = params.set('excludeId', excludeId);
    }
    return this.http.get<ApiResponse<boolean>>(`${this.apiUrl}/CheckProductName`, { params }).pipe(
      map(res => ({ exists: res.data ?? false }))
    );
  }

  getAllProductsWithUserIds(
    pageNumber: number,
    pageSize: number,
    searchTerm?: string,
    minPrice?: number,
    maxPrice?: number,
    startDate?: string,
    sortField?: string,
    sortOrder?: string,
  ): Observable<IPagedResult<IMergedProduct>> {
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

    return this.http.get<ApiResponse<IPagedResult<IMergedProduct>>>(`${environment.apiUrl}/products-with-user`, {
      params,
    }).pipe(
      map(res => res.data!)
    );
  }
}
