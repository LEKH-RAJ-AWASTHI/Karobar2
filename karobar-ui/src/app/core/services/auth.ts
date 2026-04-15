import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AuthResponse {
  token: string;
  email: string;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  TOKEN_KEY: string = 'jwt_token';

  // Track auth state
  private currentUserSubject = new BehaviorSubject<AuthResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor() {
    // Check if token exists on init
    const token = localStorage.getItem(this.TOKEN_KEY);
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        
        // ASP.NET Identity claim URIs
        const roleClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
        const emailClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
        
        let roles = payload.roles || payload[roleClaim] || [];
        if (typeof roles === 'string') roles = [roles]; // Handle single role string

        this.currentUserSubject.next({
          token,
          email: payload.email || payload[emailClaim] || '',
          roles: roles
        });
      } catch (error) {
        console.error('Token decoding failed', error);
        this.logout();
      }
    }
  }

  login(credentials: { email: string; password: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, credentials)
      .pipe(
        tap(response => {
          if (response && response.token) {
            localStorage.setItem(this.TOKEN_KEY, response.token);
            this.currentUserSubject.next(response);
          }
        })
      );
  }

  register(userData: any): Observable<any> {
    return this.http.post(`\${environment.apiUrl}/auth/register`, userData);
  }

  logout() {
    localStorage.removeItem(this.TOKEN_KEY);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }
  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const expiry = payload.exp * 1000;

      if (Date.now() >= expiry) {
        this.logout();
        return false;
      }

      return true;
    } catch {
      this.logout();
      return false;
    }
  }
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

}
