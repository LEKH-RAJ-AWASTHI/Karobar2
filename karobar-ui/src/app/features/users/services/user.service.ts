import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { User, CreateUserRequest, UpdateUserRequest } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/users`;
  private authUrl = `${environment.apiUrl}/Auth`;

  getUsers(params?: { search?: string; page?: number; size?: number }): Observable<any> {
    let httpParams = new HttpParams();
    if (params?.search) httpParams = httpParams.set('search', params.search);
    if (params?.page) httpParams = httpParams.set('page', params.page.toString());
    if (params?.size) httpParams = httpParams.set('size', params.size.toString());

    return this.http.get<any>(this.apiUrl, { params: httpParams });
  }

  getUserById(id: string): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/${id}`);
  }

  createUser(userData: CreateUserRequest): Observable<any> {
    // Note: Per user prompt, use Auth/register for user creation/invite integration
    return this.http.post(`${this.authUrl}/register`, userData);
  }

  updateUser(id: string, userData: UpdateUserRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, userData);
  }

  deleteUser(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
