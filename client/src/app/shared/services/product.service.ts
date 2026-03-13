import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { map } from 'rxjs/operators';

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
    return this.http.get<IPagedResult<IProduct>>(this.apiUrl, { params });
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

    return this.http.get<IPagedResult<IProduct>>(`${this.apiUrl}/GetAllProducts`, { params });
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

    return this.http.get<IPagedResult<IProduct>>(`${this.apiUrl}/GetMyProducts`, { params });
  }

  getById(id: string): Observable<IProduct> {
    return this.http.get<IProduct>(`${this.apiUrl}/GetById/${id}`);
  }

  getByUserId(userId: number): Observable<IProduct[]> {
    return this.http.get<IProduct[]>(`${this.apiUrl}/user/${userId}`);
  }

  create(product: IProduct): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(`${this.apiUrl}/Create`, product);
  }

  update(id: string, product: IProduct): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/Update/${id}`, { ...product, id });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/Delete/${id}`);
  }

  uploadImage(file: File): Observable<{ imageUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ imageUrl: string }>(`${this.apiUrl}/UploadImage`, formData);
  }

  checkProductName(name: string, excludeId?: string): Observable<{ exists: boolean }> {
    let params = new HttpParams().set('name', name);
    if (excludeId) {
      params = params.set('excludeId', excludeId);
    }
    return this.http.get<{ exists: boolean }>(`${this.apiUrl}/CheckProductName`, { params });
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

    return this.http.get<IPagedResult<IMergedProduct>>(`${environment.apiUrl}/products-with-user`, {
      params,
    });
  }
}
