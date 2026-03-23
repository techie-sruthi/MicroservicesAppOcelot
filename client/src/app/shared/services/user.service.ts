import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

export interface IUser {
  id: number;
  userName: string;
  email: string;
  role: string;
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
  providedIn: 'root'
})
export class UserService {

  private apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) { }

  getAllUsers(
    pageNumber: number = 1, 
    pageSize: number = 10,
    searchTerm?: string,
    roleFilter?: string,
    sortField?: string,
    sortOrder?: string
  ): Observable<IPagedResult<IUser>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
      
    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (roleFilter && roleFilter !== 'all') {
      params = params.set('roleFilter', roleFilter);
    }
    if (sortField) {
      params = params.set('sortField', sortField);
    }
    if (sortOrder) {
      params = params.set('sortOrder', sortOrder);
    }
    
    return this.http.get<ApiResponse<IPagedResult<IUser>>>(`${this.apiUrl}/GetAll`, { params }).pipe(
      map(res => res.data!)
    );
  }

  deleteUser(id: number): Observable<ApiResponse<object>> {
    return this.http.delete<ApiResponse<object>>(`${this.apiUrl}/Delete/${id}`);
  }

  updateUser(id: number, user: IUser): Observable<ApiResponse<object>> {
    return this.http.put<ApiResponse<object>>(`${this.apiUrl}/Update/${id}`, user);
  }

  createUser(user: IUser): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.apiUrl}/Create`, user);
  }

  changePassword(currentPassword: string, newPassword: string): Observable<ApiResponse<{ message: string }>> {
    return this.http.post<ApiResponse<{ message: string }>>(`${this.apiUrl}/ChangePassword`, { currentPassword, newPassword });
  }
}
