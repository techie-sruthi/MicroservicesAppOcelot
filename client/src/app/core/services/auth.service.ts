import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

interface OtpRequiredResponse {
  email: string;
  message: string;
  otpRequired: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient, private router: Router) { }

  login(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, data);
  }

  verifyOtp(email: string, otp: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/verify-otp`, { email, otp })
      .pipe(
        tap(response => {
          if (response.accessToken && response.refreshToken) {
            this.setTokens(response.accessToken, response.refreshToken);
          }
        })
      );
  }

  register(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  setTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem('access_token', accessToken);
    localStorage.setItem('refresh_token', refreshToken);
  }

  getAccessToken(): string | null {
    return localStorage.getItem('access_token');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refresh_token');
  }

  getToken(): string | null {
    return this.getAccessToken();
  }

  getUserRole(): string | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] 
          || payload['role'] 
          || payload['Role'] 
          || null;
    } catch (e) {
      return null;
    }
  }

  getUserEmail(): string | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload['email'] || payload['Email'] || null;
    } catch (e) {
      return null;
    }
  }

  getUserId(): number | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const userId = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] 
                  || payload['sub'] 
                  || payload['nameid'] 
                  || payload['userId'];
      return userId ? parseInt(userId, 10) : null;
    } catch (e) {
      return null;
    }
  }

  refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    return this.http.post<LoginResponse>(`${this.apiUrl}/refresh`, { refreshToken })
      .pipe(
        tap(response => {
          if (response.accessToken && response.refreshToken) {
            this.setTokens(response.accessToken, response.refreshToken);
          }
        })
      );
  }

  logout(): void {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user');
    sessionStorage.clear();
    this.router.navigate(['/login']);
  }

  /**
   * Check if email already exists
   * Returns true if email is already taken
   */
  checkEmail(email: string): Observable<{ exists: boolean }> {
    return this.http.get<{ exists: boolean }>(`${this.apiUrl}/check-email`, {
      params: { email }
    });
  }

  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp;
      const now = Math.floor(Date.now() / 1000);
      return expiry >= now;
    } catch (e) {
      return false;
    }
  }

  hasRole(role: string): boolean {
    return this.getUserRole() === role;
  }

  isLoggedIn(): boolean {
    return this.isAuthenticated();
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(token: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, { token, newPassword });
  }
}
