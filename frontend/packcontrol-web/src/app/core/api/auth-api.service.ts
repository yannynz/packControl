import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthUser } from '../models/auth-user.model';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);

  login(email: string, password: string): Observable<AuthUser> {
    return this.http.post<AuthUser>(
      '/api/auth/login',
      { email, password },
      { withCredentials: true }
    );
  }

  me(): Observable<AuthUser> {
    return this.http.get<AuthUser>('/api/auth/me', { withCredentials: true });
  }

  logout(): Observable<void> {
    return this.http.post<void>('/api/auth/logout', {}, { withCredentials: true });
  }
}
