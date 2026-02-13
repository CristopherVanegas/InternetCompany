import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private http: HttpClient) {}

  login(username: string, password: string) {
    return this.http.post<any>(`${environment.apiUrl}/auth/login`, {
      username,
      password
    }).pipe(
      tap(response => {
        localStorage.setItem('token', response.token);
        localStorage.setItem('role', response.role);
      })
    );
  }

  logout() {
    localStorage.clear();
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}
