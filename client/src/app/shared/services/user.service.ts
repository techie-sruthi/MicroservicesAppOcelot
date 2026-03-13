import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

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
    
    return this.http.get<IPagedResult<IUser>>(`${this.apiUrl}/GetAll`, { params });
  }

  deleteUser(id: number) {
    return this.http.delete(`${this.apiUrl}/Delete/${id}`);
  }

  updateUser(id: number, user: IUser) {
    return this.http.put<IUser>(`${this.apiUrl}/Update/${id}`, user);
  }

  createUser(user: IUser): Observable<IUser> {
    return this.http.post<IUser>(`${this.apiUrl}/Create`, user);
  }

  changePassword(currentPassword: string, newPassword: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/ChangePassword`, { currentPassword, newPassword });
  }
}
